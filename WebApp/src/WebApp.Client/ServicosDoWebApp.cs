using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Client.Auth;
using WebApp.Client.Services;

namespace WebApp.Client;

/// <summary>
/// No modo Auto, um componente pode renderizar no SERVIDOR (primeiro acesso, via
/// SignalR) ou no NAVEGADOR (depois que o WebAssembly carrega). Os dois lados
/// precisam dos mesmos servicos registrados, ou a pagina quebra ao trocar de lado.
///
/// Por isso este metodo existe e e chamado nos dois Program.cs.
/// </summary>
public static class ServicosDoWebApp
{
    public static IServiceCollection AddWebAppServices(this IServiceCollection services, string apiBaseUrl)
    {
        // Scoped: no WebAssembly equivale a singleton; no servidor, um por circuito SignalR.
        services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
        services.AddScoped<TokenStore>();
        services.AddScoped<ApiClient>();
        services.AddScoped<AuthService>();

        services.AddScoped<JwtAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
        services.AddAuthorizationCore();

        return services;
    }
}
