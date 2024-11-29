namespace DistributedDictionary.ActorAbstractions.DictionaryEntry;

[Serializable]
public sealed class EntryDefinition
{
    public long Id { get; set; }
    public string Simplified { get; set; }
    public string Traditional { get; set; }
    public string Pinyin { get; set; }
    public string Definition { get; set; }
    public string Classifier { get; set; }
    public string Concept { get; set; }
    public int HskLevel { get; set; }
    public string Topic { get; set; }
    public string ParentTopic { get; set; }
    public string Notes { get; set; }
    public double Frequency { get; set; }
    public List<string> PartOfSpeech { get; set; }
}