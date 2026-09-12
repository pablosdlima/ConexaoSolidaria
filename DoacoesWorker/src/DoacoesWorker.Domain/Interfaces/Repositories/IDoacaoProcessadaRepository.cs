using DoacoesWorker.Domain.Entities;

namespace DoacoesWorker.Domain.Interfaces.Repositories;

public interface IDoacaoProcessadaRepository
{
    /// <summary>Insere no ledger. Retorna false se IdDoacao ja existia (indice unico) — sinal de reentrega.</summary>
    Task<bool> TentarRegistrarAsync(DoacaoProcessada doacao, CancellationToken ct = default);

    Task RemoverAsync(Guid idDoacao, CancellationToken ct = default);
}
