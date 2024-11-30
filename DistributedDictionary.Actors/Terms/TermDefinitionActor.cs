using DistributedDictionary.ActorAbstractions.Terms;
using DistributedDictionary.Actors.Data;
using Orleans;
using Orleans.Runtime;

namespace DistributedDictionary.Actors.Terms;

internal sealed class TermDefinitionActor(
    [PersistentState(stateName: "entryDefinition", storageName:"DefinitionsStorage")]
    IPersistentState<TermDefinitionState> entryDefinitionState,
    ReferenceDataService referenceDataService) : Grain, ITermDefinitionActor
{
    
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        if (entryDefinitionState.State?.TermDefinition is not null)
        {
            await base.OnActivateAsync(cancellationToken);
            return;
        }

        await LoadEntryDefinitionIntoStateAsync();
    }

    private async Task LoadEntryDefinitionIntoStateAsync()
    {
        var entryDefinitions = await referenceDataService
            .QueryByHeadwordAsync(this.GetPrimaryKeyString());
        
        if (entryDefinitions.Any() && entryDefinitions.FirstOrDefault() is TermDefinition entryDefinition)
        {
            entryDefinitionState.State.TermDefinition = entryDefinition;
            entryDefinitionState.WriteStateAsync().Ignore();
        }
    }
    
    public Task<TermDefinition> GetDefinitionAsync()
    {
        return Task.FromResult(entryDefinitionState.State.TermDefinition);
    }
}