using System.Security.Claims;
using CampanhasAPI.Api.Filters;
using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;

namespace CampanhasAPI.Api.Endpoints;

public static class DoacoesEndpoints
{
    public static void MapDoacoes(this IEndpointRouteBuilder route)
    {
        // RF-5: so Doador logado. O IdDoador sai da claim sub do JWT, nunca do body.
        var grupo = route.MapGroup("/api/doacoes")
            .WithTags("Doacoes (Doador)")
            .RequireAuthorization(p => p.RequireRole("Doador"));

        grupo.MapPost("/", async (DoacaoRequest request, ClaimsPrincipal user, IDoacaoAppService service, CancellationToken ct) =>
        {
            var doacao = await service.DoarAsync(IdDoUsuario(user), request, ct);
            return Results.Accepted($"/api/doacoes/{doacao.Id}", doacao);
        })
        .AddEndpointFilter<ValidationEndpointFilter<DoacaoRequest>>()
        .WithName("Doar")
        .WithSummary("Registra uma intencao de doacao e publica o evento para o Worker. Responde 202: o total da campanha e atualizado de forma assincrona.")
        .Produces<DoacaoResponse>(202)
        .ProducesValidationProblem()
        .ProducesProblem(400)
        .ProducesProblem(404);

        grupo.MapGet("/minhas", async (ClaimsPrincipal user, IDoacaoAppService service, CancellationToken ct) =>
            Results.Ok(await service.MinhasDoacoesAsync(IdDoUsuario(user), ct)))
        .WithName("MinhasDoacoes")
        .Produces<IReadOnlyList<DoacaoResponse>>(200);
    }

    private static Guid IdDoUsuario(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? throw new InvalidOperationException("Token sem claim sub."));
}
