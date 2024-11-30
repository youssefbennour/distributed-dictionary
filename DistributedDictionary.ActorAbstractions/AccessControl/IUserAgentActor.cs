using DistributedDictionary.ActorAbstractions.Terms;
using Orleans;

namespace DistributedDictionary.ActorAbstractions.AccessControl;

public interface IUserAgentActor : IGrainWithStringKey
{
    Task<List<TermDefinition>> SearchAsync();
    Task<TermDefinition> GetDefinitionAsync();
}