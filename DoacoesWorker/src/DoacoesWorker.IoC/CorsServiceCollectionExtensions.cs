using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DoacoesWorker.IoC;

/// <summary>CORS para o painel publico ser lido pelo front Blazor (outra origem).</summary>
public static class CorsServiceCollectionExtensions
{
    public const string Politica = "front";

    public static IServiceCollection AddCorsParaFront(this IServiceCollection services, IConfiguration configuration)
    {
        var origens = configuration.GetSection("Cors:Origens").Get<string[]>() ?? [];

        services.AddCors(o => o.AddPolicy(Politica, p =>
        {
            if (origens.Length == 0) return;
            p.WithOrigins(origens).AllowAnyHeader().AllowAnyMethod();
        }));

        return services;
    }

    public static IApplicationBuilder UseCorsParaFront(this IApplicationBuilder app) => app.UseCors(Politica);
}
