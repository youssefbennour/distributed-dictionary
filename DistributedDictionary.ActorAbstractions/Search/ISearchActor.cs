using DistributedDictionary.ActorAbstractions.DictionaryEntry;
using Orleans;

namespace DistributedDictionary.ActorAbstractions.Search;

public interface ISearchActor : IGrainWithStringKey
{
    Task<List<EntryDefinition>> SearchAsync();
}