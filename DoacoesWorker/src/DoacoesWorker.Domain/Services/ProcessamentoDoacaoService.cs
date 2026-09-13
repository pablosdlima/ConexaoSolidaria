using DoacoesWorker.Domain.Entities;
using DoacoesWorker.Domain.Events;
using DoacoesWorker.Domain.Interfaces.Repositories;
using DoacoesWorker.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace DoacoesWorker.Domain.Services;

/// <summary>
/// O unico lugar do sistema que escreve ValorTotalArrecadado.
///
/// Ordem: ledger primeiro (indice unico decide quem processa), depois o $inc.
/// Se o $inc falhar, o registro do ledger e removido para a reentrega tentar de novo.
/// </summary>
public class ProcessamentoDoacaoService(
    IDoacaoProcessadaRepository ledger,
    ICampanhaResumoRepository resumos,
    IMetricas metricas,
    ILogger<ProcessamentoDoacaoService> logger) : IProcessamentoDoacaoService
{
    public async Task<bool> ProcessarAsync(DoacaoRecebidaEvent e, CancellationToken ct = default)
    {
        using var _ = metricas.MedirProcessamento();

        var registro = new DoacaoProcessada
        {
            IdDoacao = e.IdDoacao,
            IdCampanha = e.IdCampanha,
            IdDoador = e.IdDoador,
            Valor = e.Valor,
            DataHoraDoacao = e.DataHora,
            ProcessadaEm = DateTimeOffset.UtcNow
        };

        if (!await ledger.TentarRegistrarAsync(registro, ct))
        {
            metricas.DoacaoDuplicadaIgnorada();
            logger.LogWarning("Doacao {IdDoacao} ja processada — reentrega ignorada (idempotencia)", e.IdDoacao);
            return false;
        }

        try
        {
            await resumos.IncrementarArrecadadoAsync(e.IdCampanha, e.Valor, ct);
        }
        catch
        {
            await ledger.RemoverAsync(e.IdDoacao, CancellationToken.None);
            throw;
        }

        metricas.DoacaoProcessada(e.Valor);
        logger.LogInformation("Doacao {IdDoacao} somada | Campanha: {IdCampanha} | +{Valor}", e.IdDoacao, e.IdCampanha, e.Valor);
        return true;
    }
}
