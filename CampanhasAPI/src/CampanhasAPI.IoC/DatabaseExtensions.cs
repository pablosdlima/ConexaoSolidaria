using CampanhasAPI.Data;
using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.IoC;

public static class DatabaseExtensions
{
    /// <summary>
    /// Aplica migrations pendentes e garante um GestorONG para a demo
    /// (mesmo padrao de auto-migrate da Fase 3).
    /// </summary>
    public static async Task MigrateAndSeedAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

        var db = sp.GetRequiredService<CampanhasDbContext>();
        await db.Database.MigrateAsync();
        logger.LogInformation("Migrations aplicadas.");

        var config = sp.GetRequiredService<IConfiguration>();
        var email = config["Seed:Gestor:Email"];
        var senha = config["Seed:Gestor:Senha"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            logger.LogWarning("Seed:Gestor nao configurado — nenhum GestorONG criado.");
            return;
        }

        var usuarios = sp.GetRequiredService<IUsuarioRepository>();
        if (await usuarios.EmailJaCadastradoAsync(email)) return;

        var hasher = sp.GetRequiredService<ISenhaHasher>();
        var gestor = Usuario.Criar(
            config["Seed:Gestor:Nome"] ?? "Gestor da ONG",
            email,
            config["Seed:Gestor:Cpf"] ?? "52998224725",
            hasher.Hash(senha),
            Role.GestorONG);

        await usuarios.AdicionarAsync(gestor);
        logger.LogInformation("GestorONG criado para a demo | Email: {Email}", email);
    }
}
