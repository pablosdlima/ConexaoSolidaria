using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using WebApp.Client.Services;

namespace WebApp.Client.Auth;

/// <summary>
/// Diz ao Blazor quem esta logado, lendo o JWT guardado no navegador.
///
/// O token NAO e validado aqui — validar assinatura no cliente nao protege nada,
/// porque o navegador esta sob controle do usuario. A autorizacao de verdade
/// acontece na API. Aqui as claims servem so para a interface: mostrar o nome,
/// esconder o menu do gestor, redirecionar quem nao esta logado.
/// </summary>
public class JwtAuthenticationStateProvider(TokenStore tokens) : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonimo = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await tokens.ObterAsync();
        if (string.IsNullOrWhiteSpace(token)) return Anonimo;

        var claims = LerClaims(token);
        if (claims is null)
        {
            await tokens.LimparAsync();
            return Anonimo;
        }

        // Token expirado: limpa e trata como anonimo (a API recusaria de qualquer forma)
        var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (long.TryParse(exp, out var epoch) &&
            DateTimeOffset.FromUnixTimeSeconds(epoch) <= DateTimeOffset.UtcNow)
        {
            await tokens.LimparAsync();
            return Anonimo;
        }

        // Nomes curtos, como vem no JWT ("name", "role") — nao os URIs de ClaimTypes.
        var identity = new ClaimsIdentity(claims, "jwt", "name", "role");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    /// <summary>Chamado depois do login e do logout para a interface reagir.</summary>
    public void NotificarMudanca() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    /// <summary>Decodifica o payload do JWT (base64url). Nao valida assinatura — ver resumo da classe.</summary>
    private static List<Claim>? LerClaims(string token)
    {
        try
        {
            var partes = token.Split('.');
            if (partes.Length != 3) return null;

            var payload = partes[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                Convert.FromBase64String(payload));
            if (json is null) return null;

            var claims = new List<Claim>();
            foreach (var par in json)
            {
                if (par.Value.ValueKind == JsonValueKind.Array)
                    claims.AddRange(par.Value.EnumerateArray().Select(v => new Claim(par.Key, v.ToString())));
                else
                    claims.Add(new Claim(par.Key, par.Value.ToString()));
            }
            return claims;
        }
        catch
        {
            return null;   // token corrompido no localStorage
        }
    }
}
