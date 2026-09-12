using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Domain.Interfaces.Repositories;

public interface ICampanhaRepository
{
    Task<Campanha?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Campanha>> ListarAsync(CancellationToken ct = default);
    Task AdicionarAsync(Campanha campanha, CancellationToken ct = default);
    Task AtualizarAsync(Campanha campanha, CancellationToken ct = default);
}
