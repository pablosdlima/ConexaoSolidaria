using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Domain.Interfaces.Repositories;

public interface IDoacaoRepository
{
    Task AdicionarAsync(Doacao doacao, CancellationToken ct = default);
    Task AtualizarAsync(Doacao doacao, CancellationToken ct = default);
    Task<IReadOnlyList<Doacao>> ListarPorDoadorAsync(Guid idDoador, CancellationToken ct = default);
}
