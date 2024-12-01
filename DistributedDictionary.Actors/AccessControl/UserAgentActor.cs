using System.Diagnostics;
using System.Runtime.Serialization;
using DistributedDictionary.ActorAbstractions.AccessControl;
using DistributedDictionary.ActorAbstractions.Search;
using DistributedDictionary.ActorAbstractions.Terms;

namespace DistributedDictionary.Actors.AccessControl;

public sealed class UserAgentActor(IGrainFactory grainFactory) : Grain, IUserAgentActor, IIncomingGrainCallFilter
{
    private const int ThrottleThreshold = 3;
    private const int DecayPeriod = 5;
    private const double DecayRate = (double)ThrottleThreshold / (double)DecayPeriod;
    private double _throttleScore;
    private readonly Stopwatch _stopwatch = new();

    public async Task<List<TermDefinition>> SearchAsync(string searchTerm)
    {
        return await grainFactory.GetGrain<ISearchActor>(searchTerm).SearchAsync();
    }

    public async Task<TermDefinition> GetDefinitionAsync(string term)
    {
        return await grainFactory.GetGrain<ITermDefinitionActor>(term).GetDefinitionAsync();
    }

    public async Task UpdateDefinitionAsync(string term, TermDefinition termDefinition)
    {
        await grainFactory.GetGrain<ITermDefinitionActor>(term).UpdateDefinitionAsync(termDefinition);
    }

    public async Task Invoke(IIncomingGrainCallContext context)
    {
        var elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
        _stopwatch.Restart();

        _throttleScore = Math.Max(0, _throttleScore - elapsedSeconds * DecayRate) + 1;

        if (_throttleScore > ThrottleThreshold)
        {
            var remainingSeconds = Math.Max(0, (int)Math.Ceiling((_throttleScore - (ThrottleThreshold - 1)) / DecayRate));
            throw new ThrottlingException($"Request rate exceeded, wait {remainingSeconds}s before retrying"); 
        }

        await context.Invoke();    
    }
}

[GenerateSerializer]
[Alias("DistributedDictionary.Actors.AccessControl.ThrottlingException")]
public class ThrottlingException(string message) : Exception(message);