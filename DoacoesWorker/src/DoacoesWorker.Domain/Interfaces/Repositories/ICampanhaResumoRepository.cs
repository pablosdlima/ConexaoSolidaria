using DoacoesWorker.Domain.Entities;
using DoacoesWorker.Domain.Events;

namespace DoacoesWorker.Domain.Interfaces.Repositories;

public interface ICampanhaResumoRepository
{
    Task<CampanhaResumo?> ObterAsync(Guid idCampanha, CancellationToken ct = default);
    Task<IReadOnlyList<CampanhaResumo>> ListarAtivasAsync(CancellationToken ct = default);

    /// <summary>Upsert dos dados vindos da API. Nunca toca em ValorTotalArrecadado.</summary>
    Task AtualizarDadosAsync(CampanhaAtualizadaEvent evento, CancellationToken ct = default);

    /// <summary>Soma atomica ($inc). Cria o documento se ainda nao existir — o evento da campanha pode chegar depois.</summary>
    Task IncrementarArrecadadoAsync(Guid idCampanha, decimal valor, CancellationToken ct = default);
}
