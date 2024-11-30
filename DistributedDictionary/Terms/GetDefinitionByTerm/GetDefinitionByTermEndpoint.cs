using DistributedDictionary.ActorAbstractions.Terms;
using Microsoft.AspNetCore.Mvc;

namespace DistributedDictionary.Terms.GetDefinitionByTerm;

internal static class GetDefinitionByTermEndpoint
{
   public static void  MapGetDefinitionByTerm(this IEndpointRouteBuilder app) => app.MapGet(
      "api/definitions/{term}",
      async ([FromRoute] string term, 
          IGrainFactory grainFactory,
          CancellationToken cancellationToken) =>
      {
          var termGrain = grainFactory.GetGrain<ITermDefinitionActor>(term);
          var termDefinition = await termGrain.GetDefinitionAsync();
          return Results.Ok(termDefinition);
      })
       .WithOpenApi(); 
}