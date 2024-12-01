using DistributedDictionary.ActorAbstractions.AccessControl;
using DistributedDictionary.ActorAbstractions.Terms;
using Microsoft.AspNetCore.Mvc;

namespace DistributedDictionary.Terms.GetDefinitionByTerm;

internal static class GetDefinitionByTermEndpoint
{
   public static void  MapGetDefinitionByTerm(this IEndpointRouteBuilder app) => app.MapGet(
      "api/definitions/{term}",
      async ([FromRoute] string term, 
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
          var termDefinition = await userAgentGrain.GetDefinitionAsync(term);
          return Results.Ok(termDefinition);
      })
       .WithOpenApi(); 
}