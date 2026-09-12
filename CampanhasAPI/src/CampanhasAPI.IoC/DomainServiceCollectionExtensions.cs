using CampanhasAPI.Domain.Interfaces.Services;
using CampanhasAPI.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CampanhasAPI.IoC;

public static class DomainServiceCollectionExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<ICampanhaService, CampanhaService>();
        services.AddScoped<IDoacaoService, DoacaoService>();
        return services;
    }
}
