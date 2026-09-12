using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampanhasAPI.Data.Repositories;

public class DoacaoRepository(CampanhasDbContext context) : IDoacaoRepository
{
    public async Task AdicionarAsync(Doacao doacao, CancellationToken ct = default)
    {
        await context.Doacoes.AddAsync(doacao, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Doacao doacao, CancellationToken ct = default)
    {
        context.Doacoes.Update(doacao);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Doacao>> ListarPorDoadorAsync(Guid idDoador, CancellationToken ct = default) =>
        await context.Doacoes.AsNoTracking()
            .Where(d => d.IdDoador == idDoador)
            .OrderByDescending(d => d.DataHora)
            .ToListAsync(ct);
}
