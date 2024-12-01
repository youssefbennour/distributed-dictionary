using Orleans;

namespace DistributedDictionary.ActorAbstractions.Terms;

public interface ITermDefinitionActor : IGrainWithStringKey
{
    Task<TermDefinition> GetDefinitionAsync();
    Task UpdateDefinitionAsync(TermDefinition termDefinition);
}