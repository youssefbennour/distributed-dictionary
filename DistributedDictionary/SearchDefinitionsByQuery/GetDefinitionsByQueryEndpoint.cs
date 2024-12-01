using DistributedDictionary.ActorAbstractions.AccessControl;
using DistributedDictionary.ActorAbstractions.Search;
using Microsoft.AspNetCore.Mvc;

namespace DistributedDictionary.SearchDefinitionsByQuery;

internal static class GetDefinitionsByQueryEndpoint
{
    public static void  MapGetDefinitionsByQuery(this IEndpointRouteBuilder app) => app.MapGet(
            "api/definitions",
            async ([FromQuery] string searchTerm, 
                IGrainFactory grainFactory,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                var clientIp = httpContext.Connection.RemoteIpAddress?
                    .MapToIPv4()
                    .ToString();
                if (clientIp is null)
                {
                    throw new InvalidOperationException("Cannot process non TCP request");
                }
                var userAgentGrain = grainFactory.GetGrain<IUserAgentActor>(clientIp);
                var termDefinition = await userAgentGrain.SearchAsync(searchTerm);
                return Results.Ok(termDefinition);
            })
        .WithOpenApi(); 
}