using Orleans;

namespace DistributedDictionary.ActorAbstractions.DictionaryEntry;

internal interface IDictionaryEntryActor : IGrainWithStringKey
{
    Task<EntryDefinition> GetDefinitionAsync();
}