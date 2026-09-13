using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Domain.Interfaces.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<bool> EmailJaCadastradoAsync(string email, CancellationToken ct = default);
    Task<bool> CpfJaCadastradoAsync(string cpf, CancellationToken ct = default);
}
