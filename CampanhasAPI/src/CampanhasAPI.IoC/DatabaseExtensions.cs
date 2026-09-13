using CampanhasAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.IoC;

public static class DatabaseExtensions
{
    /// <summary>Aplica migrations pendentes e delega o seed ao DbSeeder (padrao de auto-migrate da Fase 3).</summary>
    public static async Task MigrateAndSeedAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<CampanhasDbContext>();
        await db.Database.MigrateAsync();
        sp.GetRequiredService<ILoggerFactory>().CreateLogger("Startup").LogInformation("Migrations aplicadas.");

        await sp.GetRequiredService<DbSeeder>().SeedGestorAsync();
    }
}
