using CampanhasAPI.Api.Filters;
using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;

namespace CampanhasAPI.Api.Endpoints;

public static class DoadoresEndpoints
{
    public static void MapDoadores(this IEndpointRouteBuilder route)
    {
        var grupo = route.MapGroup("/api/doadores").WithTags("Doadores");

        // RF-3: cadastro publico
        grupo.MapPost("/", async (CadastrarDoadorRequest request, IUsuarioAppService service, CancellationToken ct) =>
        {
            var doador = await service.CadastrarDoadorAsync(request, ct);
            return Results.Created($"/api/doadores/{doador.Id}", doador);
        })
        .AddEndpointFilter<ValidationEndpointFilter<CadastrarDoadorRequest>>()
        .AllowAnonymous()
        .WithName("CadastrarDoador")
        .WithSummary("Cadastra um novo doador (acesso publico).")
        .Produces<UsuarioResponse>(201)
        .ProducesValidationProblem()
        .ProducesProblem(409);
    }
}
