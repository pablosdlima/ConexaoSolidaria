using DoacoesWorker.Application.Dtos;
using DoacoesWorker.Application.Interfaces;

namespace DoacoesWorker.Worker.Endpoints;

public static class PainelEndpoints
{
    public static void MapPainel(this IEndpointRouteBuilder route)
    {
        // RF-4: publico, sem token, so campanhas Ativa, com o total arrecadado calculado pelo Worker
        route.MapGet("/publico/campanhas", async (IPainelAppService painel, CancellationToken ct) =>
            Results.Ok(await painel.ListarCampanhasAtivasAsync(ct)))
        .WithTags("Painel de transparencia (publico)")
        .WithName("PainelCampanhasAtivas")
        .WithSummary("Campanhas ativas com Titulo, MetaFinanceira e ValorTotalArrecadado.")
        .Produces<IReadOnlyList<CampanhaPublicaResponse>>(200);
    }
}
