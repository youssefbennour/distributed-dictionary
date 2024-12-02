using System.Diagnostics;
using DistributedDictionary.ActorAbstractions.Search;
using DistributedDictionary.ActorAbstractions.Terms;
using DistributedDictionary.Actors.Data;

namespace DistributedDictionary.Actors.Search;

public sealed class SearchActor(
    DictionaryPersistence dictionaryPersistence,
    IGrainFactory grainFactory) : Grain, ISearchActor
{

    private List<TermDefinition>? _cachedTermDefinitions;
    private IDisposable? _cacheTimer;
    
    public async Task<List<TermDefinition>> SearchAsync()
    {
        if (_cachedTermDefinitions is not null)
        {
            return _cachedTermDefinitions;
        }
        var query = this.GetPrimaryKeyString();
        var headWords = await dictionaryPersistence.QueryHeadwordsByAnyAsync(query);
        var termDefinitionTasks = new List<Task<TermDefinition>>();
        headWords.Take(25)
            .ToList()
            .ForEach(m =>
        {
            var termDefinitionActor = grainFactory.GetGrain<ITermDefinitionActor>(m);
            termDefinitionTasks.Add(termDefinitionActor.GetDefinitionAsync());
        });


        await Task.WhenAll(termDefinitionTasks);
        var termDefinitions = termDefinitionTasks.Select(t => t.Result)
            .ToList();
        
        _cachedTermDefinitions = termDefinitions;
        RegisterCacheTimer();
        return termDefinitions;
    }

    private void RegisterCacheTimer()
    {
        _cacheTimer = this.RegisterGrainTimer(
            ClearCache, new GrainTimerCreationOptions(TimeSpan.FromSeconds(15), Timeout.InfiniteTimeSpan));
    }

    private void DisposeCacheTimer()
    {
        if (_cacheTimer is not null)
        {
            _cacheTimer.Dispose();
        }
    } 

    private Task ClearCache()
    {
        _cachedTermDefinitions = null;
        return Task.CompletedTask;
    }
}