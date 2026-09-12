using DoacoesWorker.Domain.Entities;
using DoacoesWorker.Domain.Events;

namespace DoacoesWorker.Domain.Interfaces.Services;

public interface IResumoCampanhaService
{
    Task AtualizarAsync(CampanhaAtualizadaEvent evento, CancellationToken ct = default);
    Task<IReadOnlyList<CampanhaResumo>> ListarAtivasAsync(CancellationToken ct = default);
}
