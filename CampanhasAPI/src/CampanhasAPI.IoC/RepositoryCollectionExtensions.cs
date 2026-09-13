using CampanhasAPI.Data;
using CampanhasAPI.Data.Repositories;
using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CampanhasAPI.IoC;

public static class RepositoryCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Campanhas")
            ?? throw new InvalidOperationException("ConnectionStrings:Campanhas nao configurada.");

        services.AddDbContext<CampanhasDbContext>(o => o.UseNpgsql(connectionString));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.Secao));
        services.AddScoped<DbSeeder>();

        services.AddScoped<ICampanhaRepository, CampanhaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IDoacaoRepository, DoacaoRepository>();

        return services;
    }
}
