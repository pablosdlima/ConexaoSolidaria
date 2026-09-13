namespace CampanhasAPI.Domain.Interfaces.Repositories;

/// <summary>
/// Operacoes comuns a todo agregado. Os repositorios especificos herdam daqui e
/// acrescentam so as consultas que sao deles (ISP: ninguem recebe metodo que nao usa).
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(T entidade, CancellationToken ct = default);
    Task AtualizarAsync(T entidade, CancellationToken ct = default);
}
