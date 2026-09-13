using DoacoesWorker.Domain.Entities;
using DoacoesWorker.Domain.Events;
using DoacoesWorker.Domain.Interfaces.Repositories;
using DoacoesWorker.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DoacoesWorker.Domain.Services;

public class ResumoCampanhaService(
    ICampanhaResumoRepository resumos,
    IMetricas metricas,
    ILogger<ResumoCampanhaService> logger) : IResumoCampanhaService
{
    public async Task AtualizarAsync(CampanhaAtualizadaEvent e, CancellationToken ct = default)
    {
        await resumos.AtualizarDadosAsync(e, ct);
        metricas.ResumoAtualizado();
        logger.LogInformation("Resumo atualizado | Campanha: {IdCampanha} | {Titulo} | {Status}", e.IdCampanha, e.Titulo, e.Status);
    }

    public Task<IReadOnlyList<CampanhaResumo>> ListarAtivasAsync(CancellationToken ct = default) =>
        resumos.ListarAtivasAsync(ct);
}
