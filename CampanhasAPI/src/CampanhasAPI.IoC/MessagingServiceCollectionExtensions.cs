using CampanhasAPI.Domain.Interfaces.Events;
using CampanhasAPI.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace CampanhasAPI.IoC;

public static class MessagingServiceCollectionExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        // Fase 3 troca por RabbitMQEventPublisher (Singleton, mantem a conexao aberta)
        services.AddSingleton<IEventPublisher, LogEventPublisher>();
        return services;
    }
}
