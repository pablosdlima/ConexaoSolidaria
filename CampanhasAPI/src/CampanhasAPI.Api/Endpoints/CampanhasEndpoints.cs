using CampanhasAPI.Api.Filters;
using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;

namespace CampanhasAPI.Api.Endpoints;

public static class CampanhasEndpoints
{
    public static void MapCampanhas(this IEndpointRouteBuilder route)
    {
        // RF-2: gestao de campanhas — todo o grupo exige a role GestorONG.
        // Um Doador autenticado recebe 403; sem token, 401.
        var grupo = route.MapGroup("/api/campanhas")
            .WithTags("Campanhas (GestorONG)")
            .RequireAuthorization(p => p.RequireRole("GestorONG"));

        grupo.MapGet("/", async (ICampanhaAppService service, CancellationToken ct) =>
            Results.Ok(await service.ListarAsync(ct)))
        .WithName("ListarCampanhas")
        .Produces<IReadOnlyList<CampanhaResponse>>(200);

        grupo.MapGet("/{id:guid}", async (Guid id, ICampanhaAppService service, CancellationToken ct) =>
            Results.Ok(await service.ObterAsync(id, ct)))
        .WithName("ObterCampanha")
        .Produces<CampanhaResponse>(200)
        .ProducesProblem(404);

        grupo.MapPost("/", async (CampanhaRequest request, ICampanhaAppService service, CancellationToken ct) =>
        {
            var campanha = await service.CriarAsync(request, ct);
            return Results.Created($"/api/campanhas/{campanha.Id}", campanha);
        })
        .AddEndpointFilter<ValidationEndpointFilter<CampanhaRequest>>()
        .WithName("CriarCampanha")
        .WithSummary("Cria uma campanha. DataFim nao pode estar no passado; MetaFinanceira > 0.")
        .Produces<CampanhaResponse>(201)
        .ProducesValidationProblem()
        .ProducesProblem(400);

        grupo.MapPut("/{id:guid}", async (Guid id, CampanhaRequest request, ICampanhaAppService service, CancellationToken ct) =>
            Results.Ok(await service.AtualizarAsync(id, request, ct)))
        .AddEndpointFilter<ValidationEndpointFilter<CampanhaRequest>>()
        .WithName("AtualizarCampanha")
        .Produces<CampanhaResponse>(200)
        .ProducesValidationProblem()
        .ProducesProblem(400)
        .ProducesProblem(404);

        grupo.MapPatch("/{id:guid}/status", async (Guid id, AlterarStatusCampanhaRequest request, ICampanhaAppService service, CancellationToken ct) =>
            Results.Ok(await service.AlterarStatusAsync(id, request, ct)))
        .AddEndpointFilter<ValidationEndpointFilter<AlterarStatusCampanhaRequest>>()
        .WithName("AlterarStatusCampanha")
        .WithSummary("Conclui ou cancela uma campanha ativa.")
        .Produces<CampanhaResponse>(200)
        .ProducesValidationProblem()
        .ProducesProblem(400)
        .ProducesProblem(404);
    }
}
