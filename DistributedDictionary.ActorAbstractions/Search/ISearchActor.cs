using DistributedDictionary.ActorAbstractions.Terms;
using Orleans;

namespace DistributedDictionary.ActorAbstractions.Search;

public interface ISearchActor : IGrainWithStringKey
{
    Task<List<TermDefinition>> SearchAsync();
}