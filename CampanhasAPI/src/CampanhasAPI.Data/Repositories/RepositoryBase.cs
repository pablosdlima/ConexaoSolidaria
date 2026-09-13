using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampanhasAPI.Data.Repositories;

/// <summary>
/// Implementacao unica das operacoes comuns (evita repetir Add/Update/SaveChanges em
/// cada repositorio). Aberta para extensao: os filhos ganham o DbSet protegido e
/// acrescentam consultas sem tocar aqui (OCP).
/// </summary>
public abstract class RepositoryBase<T>(CampanhasDbContext context) : IRepository<T> where T : class
{
    protected readonly CampanhasDbContext Context = context;
    protected readonly DbSet<T> Set = context.Set<T>();

    public virtual async Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public virtual async Task AdicionarAsync(T entidade, CancellationToken ct = default)
    {
        await Set.AddAsync(entidade, ct);
        await Context.SaveChangesAsync(ct);
    }

    public virtual async Task AtualizarAsync(T entidade, CancellationToken ct = default)
    {
        Set.Update(entidade);
        await Context.SaveChangesAsync(ct);
    }
}
