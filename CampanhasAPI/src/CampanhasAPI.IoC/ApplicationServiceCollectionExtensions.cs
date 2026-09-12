using CampanhasAPI.Application.AppServices;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Application.Security;
using CampanhasAPI.Application.Validators;
using CampanhasAPI.Domain.Interfaces.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CampanhasAPI.IoC;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.Secao));

        services.AddScoped<IUsuarioAppService, UsuarioAppService>();
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<ICampanhaAppService, CampanhaAppService>();
        services.AddScoped<IDoacaoAppService, DoacaoAppService>();

        services.AddSingleton<ISenhaHasher, BCryptSenhaHasher>();
        services.AddSingleton<IJwtGenerator, JwtGenerator>();

        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;
    }
}
