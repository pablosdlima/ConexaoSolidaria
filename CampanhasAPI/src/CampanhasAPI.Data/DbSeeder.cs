using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CampanhasAPI.Data;

/// <summary>
/// Garante um GestorONG para a demo. So faz isso (SRP) — quem aplica migrations e
/// o DatabaseExtensions. Idempotente: nao cria se o e-mail ja existe.
/// </summary>
public class DbSeeder(
    IUsuarioRepository usuarios,
    ISenhaHasher hasher,
    IOptions<SeedOptions> options,
    ILogger<DbSeeder> logger)
{
    public async Task SeedGestorAsync(CancellationToken ct = default)
    {
        var seed = options.Value;
        if (string.IsNullOrWhiteSpace(seed.Email) || string.IsNullOrWhiteSpace(seed.Senha))
        {
            logger.LogWarning("Seed:Gestor nao configurado — nenhum GestorONG criado.");
            return;
        }

        if (await usuarios.EmailJaCadastradoAsync(seed.Email, ct)) return;

        var gestor = Usuario.Criar(seed.Nome, seed.Email, seed.Cpf, hasher.Hash(seed.Senha), Role.GestorONG);
        await usuarios.AdicionarAsync(gestor, ct);

        logger.LogInformation("GestorONG criado para a demo | Email: {Email}", seed.Email);
    }
}
