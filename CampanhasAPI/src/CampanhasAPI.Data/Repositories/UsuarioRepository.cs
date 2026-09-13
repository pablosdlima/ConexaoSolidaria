using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampanhasAPI.Data.Repositories;

public class UsuarioRepository(CampanhasDbContext context) : RepositoryBase<Usuario>(context), IUsuarioRepository
{
    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(u => u.Email == Normalizar(email), ct);

    public Task<bool> EmailJaCadastradoAsync(string email, CancellationToken ct = default) =>
        Set.AnyAsync(u => u.Email == Normalizar(email), ct);

    public Task<bool> CpfJaCadastradoAsync(string cpf, CancellationToken ct = default) =>
        Set.AnyAsync(u => u.Cpf == cpf, ct);

    private static string Normalizar(string email) => email.Trim().ToLowerInvariant();
}
