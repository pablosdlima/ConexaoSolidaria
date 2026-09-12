using CampanhasAPI.Api.Filters;
using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;

namespace CampanhasAPI.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder route)
    {
        var grupo = route.MapGroup("/api/auth").WithTags("Autenticacao");

        // RF-1: JWT com a claim de role
        grupo.MapPost("/login", async (LoginRequest request, IAuthAppService service, CancellationToken ct) =>
            Results.Ok(await service.LoginAsync(request, ct)))
        .AddEndpointFilter<ValidationEndpointFilter<LoginRequest>>()
        .AllowAnonymous()
        .WithName("Login")
        .WithSummary("Autentica e devolve um JWT com a role (GestorONG ou Doador).")
        .Produces<LoginResponse>(200)
        .ProducesValidationProblem()
        .ProducesProblem(401);
    }
}
