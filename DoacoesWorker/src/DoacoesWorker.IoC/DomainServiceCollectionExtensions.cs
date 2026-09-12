using DoacoesWorker.Domain.Interfaces.Services;
using DoacoesWorker.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DoacoesWorker.IoC;

public static class DomainServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IProcessamentoDoacaoService, ProcessamentoDoacaoService>();
        services.AddScoped<IResumoCampanhaService, ResumoCampanhaService>();
        return services;
    }
}
