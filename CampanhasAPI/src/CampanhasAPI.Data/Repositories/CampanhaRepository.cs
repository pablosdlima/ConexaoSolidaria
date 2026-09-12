using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampanhasAPI.Data.Repositories;

public class CampanhaRepository(CampanhasDbContext context) : ICampanhaRepository
{
    public Task<Campanha?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Campanhas.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Campanha>> ListarAsync(CancellationToken ct = default) =>
        await context.Campanhas.AsNoTracking().OrderByDescending(c => c.DataCriacao).ToListAsync(ct);

    public async Task AdicionarAsync(Campanha campanha, CancellationToken ct = default)
    {
        await context.Campanhas.AddAsync(campanha, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Campanha campanha, CancellationToken ct = default)
    {
        context.Campanhas.Update(campanha);
        await context.SaveChangesAsync(ct);
    }
}
