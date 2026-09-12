using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CampanhasAPI.Data.Repositories;

public class UsuarioRepository(CampanhasDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken ct = default) =>
        context.Usuarios.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLowerInvariant(), ct);

    public Task<bool> EmailJaCadastradoAsync(string email, CancellationToken ct = default) =>
        context.Usuarios.AnyAsync(u => u.Email == email.Trim().ToLowerInvariant(), ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
    {
        await context.Usuarios.AddAsync(usuario, ct);
        await context.SaveChangesAsync(ct);
    }
}
