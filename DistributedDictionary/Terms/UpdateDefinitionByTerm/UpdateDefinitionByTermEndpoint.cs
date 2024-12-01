using DistributedDictionary.ActorAbstractions.AccessControl;
using DistributedDictionary.ActorAbstractions.Terms;
using Microsoft.AspNetCore.Mvc;

namespace DistributedDictionary.Terms.UpdateDefinitionByTerm;

internal static class UpdateDefinitionByTermEndpoint
{
    public static void MapUpdateDefinitionByTerm(this IEndpointRouteBuilder app) => app.MapPut(
            "api/definitions/{term}",
            async ([FromRoute] string term,
                [FromBody] TermDefinition termDefinitionUpdateRequest,
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
                await userAgentGrain.UpdateDefinitionAsync(term ,termDefinitionUpdateRequest);
                
                return Results.NoContent();
            })
        .WithOpenApi();
}