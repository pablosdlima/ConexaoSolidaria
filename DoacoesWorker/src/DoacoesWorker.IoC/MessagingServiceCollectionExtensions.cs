using DoacoesWorker.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DoacoesWorker.IoC;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQOptions>(configuration.GetSection(RabbitMQOptions.Secao));
        services.AddSingleton<RabbitMQConexao>();
        services.AddSingleton<RabbitMQInitializer>();

        // Um BackgroundService por fila
        services.AddHostedService<DoacaoRecebidaRabbitConsumer>();
        services.AddHostedService<CampanhaAtualizadaRabbitConsumer>();
        return services;
    }

    public static async Task InitializeRabbitMQAsync(this IHost app)
    {
        var initializer = app.Services.GetRequiredService<RabbitMQInitializer>();
        await initializer.InitializeAsync();
    }
}
