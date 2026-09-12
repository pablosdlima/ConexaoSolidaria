using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CampanhasAPI.Data;

/// <summary>
/// Usado só pelo `dotnet ef` em tempo de design (migrations). Em execução a
/// connection string vem da configuração da API.
/// </summary>
internal class ContextFactory : IDesignTimeDbContextFactory<CampanhasDbContext>
{
    public CampanhasDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Campanhas")
            ?? "Host=localhost;Database=campanhas;Username=dev;Password=dev";

        var options = new DbContextOptionsBuilder<CampanhasDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CampanhasDbContext(options);
    }
}
