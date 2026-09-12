using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace DoacoesWorker.IoC;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Conexao Solidaria - DoacoesWorker",
            Version = "v1",
            Description = "Consome as filas do RabbitMQ e mantem o total arrecadado por campanha. " +
                          "Expoe o painel publico de transparencia (RF-4)."
        }));
        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DoacoesWorker v1"));
        return app;
    }
}
