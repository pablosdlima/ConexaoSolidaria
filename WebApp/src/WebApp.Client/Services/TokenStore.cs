using Microsoft.JSInterop;

namespace WebApp.Client.Services;

/// <summary>
/// Guarda o JWT no localStorage do navegador.
///
/// Sem biblioteca externa de proposito: sao tres chamadas de JS interop, e assim
/// fica visivel como o Blazor conversa com o navegador. Em producao, avalie usar
/// um cookie HttpOnly — localStorage e acessivel por qualquer script da pagina (XSS).
/// </summary>
public class TokenStore(IJSRuntime js)
{
    private const string Chave = "conexao-solidaria.token";
    private string? _cache;

    public async Task<string?> ObterAsync()
    {
        if (_cache is not null) return _cache;
        _cache = await js.InvokeAsync<string?>("localStorage.getItem", Chave);
        return _cache;
    }

    public async Task SalvarAsync(string token)
    {
        _cache = token;
        await js.InvokeVoidAsync("localStorage.setItem", Chave, token);
    }

    public async Task LimparAsync()
    {
        _cache = null;
        await js.InvokeVoidAsync("localStorage.removeItem", Chave);
    }
}
