using DistributedDictionary.ActorAbstractions.Terms;

namespace DistributedDictionary.Actors.Terms;
[GenerateSerializer]
internal sealed class TermDefinitionState
{
   public TermDefinition TermDefinition { get; set; } 
}