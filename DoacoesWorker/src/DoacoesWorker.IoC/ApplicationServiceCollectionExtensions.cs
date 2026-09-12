using DoacoesWorker.Application.AppServices;
using DoacoesWorker.Application.Consumers;
using DoacoesWorker.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DoacoesWorker.IoC;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPainelAppService, PainelAppService>();

        // Handlers das filas: Scoped, um por mensagem (padrao da Fase 3)
        services.AddScoped<DoacaoRecebidaConsumer>();
        services.AddScoped<CampanhaAtualizadaConsumer>();
        return services;
    }
}
