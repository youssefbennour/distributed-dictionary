using DistributedDictionary.ActorAbstractions.Terms;
using Orleans;

namespace DistributedDictionary.ActorAbstractions.AccessControl;

public interface IUserAgentActor : IGrainWithStringKey
{
    Task<List<TermDefinition>> SearchAsync(string searchTerms);
    Task<TermDefinition> GetDefinitionAsync(string term);
    Task UpdateDefinitionAsync(string term, TermDefinition termDefinition);
}