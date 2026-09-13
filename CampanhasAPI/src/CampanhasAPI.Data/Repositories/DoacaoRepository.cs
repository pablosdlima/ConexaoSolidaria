using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampanhasAPI.Data.Repositories;

public class DoacaoRepository(CampanhasDbContext context) : RepositoryBase<Doacao>(context), IDoacaoRepository
{
    public async Task<IReadOnlyList<Doacao>> ListarPorDoadorAsync(Guid idDoador, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Where(d => d.IdDoador == idDoador)
            .OrderByDescending(d => d.DataHora)
            .ToListAsync(ct);
}
