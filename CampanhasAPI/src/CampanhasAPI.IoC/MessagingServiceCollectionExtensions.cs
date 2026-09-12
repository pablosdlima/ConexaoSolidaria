using CampanhasAPI.Domain.Interfaces.Events;
using CampanhasAPI.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CampanhasAPI.IoC;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQOptions>(configuration.GetSection(RabbitMQOptions.Secao));
        services.AddSingleton<RabbitMQConexao>();
        services.AddSingleton<RabbitMQInitializer>();
        services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
        return services;
    }

    /// <summary>Declara a topologia no startup (padrão da Fase 3). Falha rápido se o broker não estiver acessível.</summary>
    public static async Task InitializeRabbitMQAsync(this IHost app)
    {
        var initializer = app.Services.GetRequiredService<RabbitMQInitializer>();
        await initializer.InitializeAsync();
    }
}
