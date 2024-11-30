using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Text.Unicode;
using DistributedDictionary.ActorAbstractions.Terms;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic.FileIO;

namespace DistributedDictionary.Actors.Data;

public class ReferenceDataService(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
    private readonly string _connectionString = new SqliteConnectionStringBuilder($"Data Source={Path.Combine(
        hostEnvironment.ContentRootPath, 
        "../DistributedDictionary.Actors/Data/hanbaobao.db")};")
    {
        Mode = SqliteOpenMode.ReadOnly,
        Cache = SqliteCacheMode.Shared,
    }.ToString(); 

    private const string Ordering = "hsk_level not null desc, hsk_level asc, part_of_speech not null desc, frequency desc";

    public async Task<List<string>> QueryHeadwordsByAnyAsync(string query, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        if (IsProbablyCjk(query))
        {
            return await QueryHeadwordByHeadwordAsync(connection, query, cancellationToken);
        }

        return await QueryHeadwordByDefinitionAsync(connection, query, cancellationToken);
    }

    public async Task<List<TermDefinition>> QueryByAnyAsync(string query, CancellationToken cancellationToken = default)
    {
        if (IsProbablyCjk(query))
        {
            return await QueryByHeadwordAsync(query, cancellationToken);
        }

        return await QueryByDefinitionAsync(query, cancellationToken);
    }

    private async Task<List<string>> QueryHeadwordByHeadwordAsync(SqliteConnection connection, string query, CancellationToken cancellationToken)
    {
        await using var cmd = new SqliteCommand(
            "SELECT DISTINCT simplified FROM dictionary WHERE simplified = $term OR traditional = $term ORDER BY " + Ordering, 
            connection);
        cmd.Parameters.AddWithValue("$term", query);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await ReadHeadwordsAsync(reader, cancellationToken);
    }

    private async Task<List<string>> QueryHeadwordByDefinitionAsync(SqliteConnection connection, string query, CancellationToken cancellationToken)
    {
        await using var cmd = new SqliteCommand(
            "SELECT DISTINCT simplified FROM dictionary WHERE rowid IN (SELECT rowid FROM fts_definition WHERE fts_definition MATCH $query) ORDER BY " + Ordering, 
            connection);
        cmd.Parameters.AddWithValue("$query", query);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await ReadHeadwordsAsync(reader, cancellationToken);
    }

    public async Task<List<TermDefinition>> QueryByHeadwordAsync(string query, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqliteCommand(
            "SELECT * FROM dictionary WHERE simplified = $term OR traditional = $term ORDER BY " + Ordering, 
            connection);
        cmd.Parameters.AddWithValue("$term", query);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await ReadAllAsEntryDefinitionAsync(reader, cancellationToken);
    }

    private async Task<List<TermDefinition>> QueryByDefinitionAsync(string query, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var cmd = new SqliteCommand(
            "SELECT * FROM dictionary WHERE rowid IN (SELECT rowid FROM fts_definition WHERE fts_definition MATCH $query) ORDER BY " + Ordering, 
            connection);
        cmd.Parameters.AddWithValue("$query", query);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        return await ReadAllAsEntryDefinitionAsync(reader, cancellationToken);
    }

    private static bool IsProbablyCjk(string text)
    {
        return text.Any(c => 
            IsInRange(UnicodeRanges.CjkUnifiedIdeographs, c) || 
            IsInRange(UnicodeRanges.CjkUnifiedIdeographsExtensionA, c));

        static bool IsInRange(UnicodeRange range, char c)
        {
            var val = (int)c;
            return val >= range.FirstCodePoint && val <= (range.FirstCodePoint + range.Length);
        }
    }

    private static async Task<List<string>> ReadHeadwordsAsync(DbDataReader reader, CancellationToken cancellationToken)
    {
        var results = new List<string>();
        int simplifiedColId = reader.GetOrdinal("simplified");

        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(simplifiedColId))
            {
                results.Add(reader.GetString(simplifiedColId));
            }
        }

        return results;
    }

    private static async Task<List<TermDefinition>> ReadAllAsEntryDefinitionAsync(DbDataReader reader, CancellationToken cancellationToken)
    {
        var results = new List<TermDefinition>();
        
        // Column ordinals
        int rowIdColId = reader.GetOrdinal("rowid");
        int simplifiedColId = reader.GetOrdinal("simplified");
        int traditionalColId = reader.GetOrdinal("traditional");
        int pinyinColId = reader.GetOrdinal("pinyin");
        int definitionColId = reader.GetOrdinal("definition");
        int hskLevelColId = reader.GetOrdinal("hsk_level");
        int classifierColId = reader.GetOrdinal("classifier");
        int posColId = reader.GetOrdinal("part_of_speech");
        int frequencyColId = reader.GetOrdinal("frequency");
        int conceptColId = reader.GetOrdinal("concept");
        int topicColId = reader.GetOrdinal("topic");
        int parentTopicColId = reader.GetOrdinal("parent_topic");
        int notesColId = reader.GetOrdinal("notes");

        while (await reader.ReadAsync(cancellationToken))
        {
            var result = new TermDefinition
            {
                Id = reader.GetInt64(rowIdColId),
                Simplified = reader.IsDBNull(simplifiedColId) ? null : reader.GetString(simplifiedColId),
                Traditional = reader.IsDBNull(traditionalColId) ? null : reader.GetString(traditionalColId),
                Pinyin = reader.IsDBNull(pinyinColId) ? null : reader.GetString(pinyinColId),
                Definition = reader.IsDBNull(definitionColId) ? null : reader.GetString(definitionColId),
                Classifier = reader.IsDBNull(classifierColId) ? null : reader.GetString(classifierColId),
                HskLevel = reader.IsDBNull(hskLevelColId) ? 0 : reader.GetInt32(hskLevelColId),
                PartOfSpeech = reader.IsDBNull(posColId) ? null : PartOfSpeechToStrings(reader.GetInt64(posColId)),
                Frequency = reader.IsDBNull(frequencyColId) ? 0 : reader.GetDouble(frequencyColId),
                Concept = reader.IsDBNull(conceptColId) ? null : reader.GetString(conceptColId),
                Topic = reader.IsDBNull(topicColId) ? null : reader.GetString(topicColId),
                ParentTopic = reader.IsDBNull(parentTopicColId) ? null : reader.GetString(parentTopicColId),
                Notes = reader.IsDBNull(notesColId) ? null : reader.GetString(notesColId)
            };
            results.Add(result);
        }

        return results;
    }

   public static List<string> PartOfSpeechToStrings(long input)
        {
            List<string> result = new List<string>();
            if ((input & ((long)1)) != 0)
            {
                result.Add("ADDRESS");
            }

            if ((input & (((long)1 << 1))) != 0)
            {
                result.Add("ADJECTIVE");
            }

            if ((input & ((long)1 << 2)) != 0)
            {
                result.Add("ADVERB");
            }

            if ((input & ((long)1 << 3)) != 0)
            {
                result.Add("AUXILIARY VERB");
            }

            if ((input & ((long)1 << 4)) != 0)
            {
                result.Add("BOUND MORPHEME");
            }

            if ((input & ((long)1 << 5)) != 0)
            {
                result.Add("SET PHRASE");
            }

            if ((input & ((long)1 << 6)) != 0)
            {
                result.Add("CITY");
            }

            if ((input & ((long)1 << 7)) != 0)
            {
                result.Add("COMPLEMENT");
            }

            if ((input & ((long)1 << 8)) != 0)
            {
                result.Add("CONJUNCTION");
            }

            if ((input & ((long)1 << 9)) != 0)
            {
                result.Add("COUNTRY");
            }

            if ((input & ((long)1 << 10)) != 0)
            {
                result.Add("DATE");
            }

            if ((input & ((long)1 << 11)) != 0)
            {
                result.Add("DETERMINER");
            }

            if ((input & ((long)1 << 12)) != 0)
            {
                result.Add("DIRECTIONAL");
            }

            if ((input & ((long)1 << 13)) != 0)
            {
                result.Add("EXPRESSION");
            }

            if ((input & ((long)1 << 14)) != 0)
            {
                result.Add("FOREIGN TERM");
            }

            if ((input & ((long)1 << 15)) != 0)
            {
                result.Add("GEOGRAPHY");
            }

            if ((input & ((long)1 << 16)) != 0)
            {
                result.Add("IDIOM");
            }

            if ((input & ((long)1 << 17)) != 0)
            {
                result.Add("INTERJECTION");
            }

            if ((input & ((long)1 << 18)) != 0)
            {
                result.Add("MEASURE WORD");
            }

            if ((input & ((long)1 << 19)) != 0)
            {
                result.Add("MEASUREMENT");
            }
            //if ((input & ((long)1 << 20)) != 0) result.Add("NAME");
            if ((input & ((long)1 << 20)) != 0 || (input & ((long)1 << 21)) != 0)
            {
                result.Add("NOUN");
            }

            if ((input & ((long)1 << 22)) != 0)
            {
                result.Add("NUMBER");
            }

            if ((input & ((long)1 << 23)) != 0)
            {
                result.Add("NUMERAL");
            }

            if ((input & ((long)1 << 24)) != 0)
            {
                result.Add("ONOMATOPOEIA");
            }

            if ((input & ((long)1 << 25)) != 0)
            {
                result.Add("ORDINAL");
            }

            if ((input & ((long)1 << 26)) != 0)
            {
                result.Add("ORGANIZATION");
            }

            if ((input & ((long)1 << 27)) != 0)
            {
                result.Add("PARTICLE");
            }

            if ((input & ((long)1 << 28)) != 0)
            {
                result.Add("PERSON");
            }

            if ((input & ((long)1 << 29)) != 0)
            {
                result.Add("PHONETIC");
            }

            if ((input & ((long)1 << 30)) != 0)
            {
                result.Add("PHRASE");
            }

            if ((input & ((long)1 << 31)) != 0)
            {
                result.Add("PLACE");
            }

            if ((input & ((long)1 << 32)) != 0)
            {
                result.Add("PREFIX");
            }

            if ((input & ((long)1 << 33)) != 0)
            {
                result.Add("PREPOSITION");
            }

            if ((input & ((long)1 << 34)) != 0)
            {
                result.Add("PRONOUN");
            }

            if ((input & ((long)1 << 35)) != 0)
            {
                result.Add("PROPER NOUN");
            }

            if ((input & ((long)1 << 36)) != 0)
            {
                result.Add("QUANTITY");
            }

            if ((input & ((long)1 << 37)) != 0)
            {
                result.Add("RADICAL");
            }

            if ((input & ((long)1 << 38)) != 0)
            {
                result.Add("SUFFIX");
            }

            if ((input & ((long)1 << 39)) != 0)
            {
                result.Add("TEMPORAL");
            }

            if ((input & ((long)1 << 40)) != 0)
            {
                result.Add("TIME");
            }

            if ((input & ((long)1 << 41)) != 0)
            {
                result.Add("VERB");
            }

            return result;
        }
}