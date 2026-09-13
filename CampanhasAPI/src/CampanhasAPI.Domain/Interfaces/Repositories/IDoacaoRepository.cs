using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Domain.Interfaces.Repositories;

public interface IDoacaoRepository : IRepository<Doacao>
{
    Task<IReadOnlyList<Doacao>> ListarPorDoadorAsync(Guid idDoador, CancellationToken ct = default);
}
