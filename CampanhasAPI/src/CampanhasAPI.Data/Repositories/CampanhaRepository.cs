using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampanhasAPI.Data.Repositories;

public class CampanhaRepository(CampanhasDbContext context) : RepositoryBase<Campanha>(context), ICampanhaRepository
{
    public async Task<IReadOnlyList<Campanha>> ListarAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().OrderByDescending(c => c.DataCriacao).ToListAsync(ct);
}
