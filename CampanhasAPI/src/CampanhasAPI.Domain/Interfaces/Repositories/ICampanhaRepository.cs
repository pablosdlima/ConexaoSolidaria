using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Domain.Interfaces.Repositories;

public interface ICampanhaRepository : IRepository<Campanha>
{
    Task<IReadOnlyList<Campanha>> ListarAsync(CancellationToken ct = default);
}
