using DoacoesWorker.Data;
using DoacoesWorker.Data.Repositories;
using DoacoesWorker.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DoacoesWorker.IoC;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection(MongoOptions.Secao));
        services.AddSingleton<MongoDbContext>();
        services.AddScoped<ICampanhaResumoRepository, CampanhaResumoRepository>();
        services.AddScoped<IDoacaoProcessadaRepository, DoacaoProcessadaRepository>();
        return services;
    }

    /// <summary>Confere a conexao e cria os indices no startup. Falha rapido se o Mongo nao estiver acessivel.</summary>
    public static async Task InitializeMongoAsync(this IHost app)
    {
        var ctx = app.Services.GetRequiredService<MongoDbContext>();
        await ctx.PingAsync();
        await ctx.GarantirIndicesAsync();
    }
}
