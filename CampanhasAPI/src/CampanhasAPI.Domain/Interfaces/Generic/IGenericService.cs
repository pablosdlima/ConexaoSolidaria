namespace CampanhasAPI.Domain.Interfaces.Generic;

/// <summary>
/// Operacoes de dominio comuns a todo agregado (mesma ideia do IGenericServices da
/// Fase 3, sem Delete e sem expor IQueryable). Os services especificos herdam e
/// acrescentam as operacoes de negocio.
/// </summary>
public interface IGenericService<T> where T : class
{
    /// <summary>Lanca NotFoundException se nao existir — a regra do 404 fica em um unico lugar.</summary>
    Task<T> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<T> AdicionarAsync(T entidade, CancellationToken ct = default);
    Task<T> AtualizarAsync(T entidade, CancellationToken ct = default);
}
