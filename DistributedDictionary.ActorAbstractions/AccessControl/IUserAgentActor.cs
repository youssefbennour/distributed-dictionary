using DistributedDictionary.ActorAbstractions.DictionaryEntry;
using Orleans;

namespace DistributedDictionary.ActorAbstractions.AccessControl;

public interface IUserAgentActor : IGrainWithStringKey
{
    Task<List<EntryDefinition>> SearchAsync();
    Task<EntryDefinition> GetDefinitionAsync();
}