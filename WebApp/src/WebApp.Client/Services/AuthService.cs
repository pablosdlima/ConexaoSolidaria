using WebApp.Client.Auth;
using WebApp.Client.Models;

namespace WebApp.Client.Services;

/// <summary>Login e logout: chama a API, guarda o token e avisa a interface.</summary>
public class AuthService(ApiClient api, TokenStore tokens, JwtAuthenticationStateProvider auth)
{
    public async Task<ResultadoApi<LoginResponse>> LoginAsync(string email, string senha)
    {
        var r = await api.LoginAsync(new LoginRequest(email, senha));
        if (r.Sucesso && r.Dados is not null)
        {
            await tokens.SalvarAsync(r.Dados.Token);
            auth.NotificarMudanca();
        }
        return r;
    }

    public async Task LogoutAsync()
    {
        await tokens.LimparAsync();
        auth.NotificarMudanca();
    }
}
