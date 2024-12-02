using DistributedDictionary.ActorAbstractions.Terms;
using DistributedDictionary.Actors.Data;
using Orleans;
using Orleans.Runtime;

namespace DistributedDictionary.Actors.Terms;

internal sealed class TermDefinitionActor(
    [PersistentState(stateName: "termDefinition", storageName:"DefinitionsStorage")]
    IPersistentState<TermDefinitionState> termDefinitionState,
    DictionaryPersistence dictionaryPersistence) : Grain, ITermDefinitionActor
{
    
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (termDefinitionState.State?.TermDefinition is not null)
        {
            await base.OnActivateAsync(cancellationToken);
            return;
        }

        await LoadTermDefinitionIntoStateAsync();
    }

    private async Task LoadTermDefinitionIntoStateAsync()
    {
        var termDefinitions = await dictionaryPersistence
            .QueryByHeadwordAsync(this.GetPrimaryKeyString());
        
        if (termDefinitions.Any() && termDefinitions.FirstOrDefault() is TermDefinition termDefinition)
        {
            termDefinitionState.State.TermDefinition = termDefinition;
            termDefinitionState.WriteStateAsync().Ignore();
        }
    }
    
    public Task<TermDefinition> GetDefinitionAsync()
    {
        return Task.FromResult(termDefinitionState.State.TermDefinition);
    }

    public async Task UpdateDefinitionAsync(TermDefinition newDefinition)
    {
        if (!string.Equals(newDefinition.Simplified, this.GetPrimaryKeyString(), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Cannot change the headword for a definition");
        }
        termDefinitionState.State.TermDefinition = newDefinition;
        await termDefinitionState.WriteStateAsync();
    }
}