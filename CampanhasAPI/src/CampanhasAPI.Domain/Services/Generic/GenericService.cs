using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Interfaces.Generic;
using CampanhasAPI.Domain.Interfaces.Repositories;

namespace CampanhasAPI.Domain.Services.Generic;

/// <summary>
/// Base dos domain services: delega o CRUD comum ao IRepository&lt;T&gt; e centraliza
/// a excecao de "nao encontrado". Aberta para extensao (metodos virtual), fechada
/// para modificacao — um service novo herda e nao toca aqui (OCP).
/// </summary>
public abstract class GenericService<T>(IRepository<T> repository) : IGenericService<T> where T : class
{
    protected readonly IRepository<T> Repository = repository;

    public virtual async Task<T> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await Repository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"{typeof(T).Name} {id} não encontrado(a).");

    public virtual async Task<T> AdicionarAsync(T entidade, CancellationToken ct = default)
    {
        await Repository.AdicionarAsync(entidade, ct);
        return entidade;
    }

    public virtual async Task<T> AtualizarAsync(T entidade, CancellationToken ct = default)
    {
        await Repository.AtualizarAsync(entidade, ct);
        return entidade;
    }
}
