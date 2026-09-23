using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CampanhasAPI.IoC;

/// <summary>
/// CORS para o front Blazor WebAssembly, que roda no navegador em outra origem.
/// Sem isso o navegador bloqueia a resposta (o servidor responde, mas o JS nao le).
/// As origens vem da configuracao — em producao, nunca AllowAnyOrigin com credenciais.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    public const string Politica = "front";

    public static IServiceCollection AddCorsParaFront(this IServiceCollection services, IConfiguration configuration)
    {
        var origens = configuration.GetSection("Cors:Origens").Get<string[]>() ?? [];

        services.AddCors(o => o.AddPolicy(Politica, p =>
        {
            if (origens.Length == 0) return;

            p.WithOrigins(origens)
             .AllowAnyHeader()      // precisa do Authorization e do Content-Type
             .AllowAnyMethod()      // GET, POST, PUT, PATCH e o preflight OPTIONS
             .WithExposedHeaders("Location");
        }));

        return services;
    }

    /// <summary>Deve vir antes de UseAuthentication: o preflight OPTIONS chega sem token.</summary>
    public static IApplicationBuilder UseCorsParaFront(this IApplicationBuilder app) =>
        app.UseCors(Politica);
}
