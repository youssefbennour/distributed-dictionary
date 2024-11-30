using Orleans;

namespace DistributedDictionary.ActorAbstractions.DictionaryEntry;

public interface IDictionaryEntryActor : IGrainWithStringKey
{
    Task<EntryDefinition> GetDefinitionAsync();
}