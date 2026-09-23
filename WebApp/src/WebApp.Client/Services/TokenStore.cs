using Microsoft.JSInterop;

namespace WebApp.Client.Services;

/// <summary>
/// Guarda o JWT no localStorage do navegador.
///
/// Sem biblioteca externa de proposito: sao tres chamadas de JS interop, e assim fica
/// visivel como o Blazor conversa com o navegador. Em producao, avalie cookie HttpOnly —
/// localStorage e legivel por qualquer script da pagina (XSS).
///
/// PONTO IMPORTANTE DO MODO AUTO: no primeiro acesso o componente renderiza no SERVIDOR,
/// e ali o JS interop ainda nao esta disponivel (so a partir de OnAfterRenderAsync).
/// Por isso as leituras toleram InvalidOperationException e devolvem "sem token" —
/// a interface aparece deslogada por um instante e o AppLayout reavalia assim que o
/// navegador assume. Sem esse cuidado, a pagina inteira falha em branco.
/// </summary>
public class TokenStore(IJSRuntime js)
{
    private const string Chave = "conexao-solidaria.token";

    private string? _cache;

    /// <summary>True quando uma leitura foi feita antes do JS estar pronto (render no servidor).</summary>
    public bool LeituraAdiada { get; private set; }

    public async Task<string?> ObterAsync()
    {
        if (_cache is not null) return _cache;

        try
        {
            _cache = await js.InvokeAsync<string?>("localStorage.getItem", Chave);
            LeituraAdiada = false;
            return _cache;
        }
        catch (InvalidOperationException)
        {
            LeituraAdiada = true;   // render estatico/servidor: tenta de novo depois
            return null;
        }
        catch (JSDisconnectedException)
        {
            return null;            // circuito encerrado (usuario fechou a aba)
        }
    }

    public async Task SalvarAsync(string token)
    {
        _cache = token;
        await js.InvokeVoidAsync("localStorage.setItem", Chave, token);
    }

    public async Task LimparAsync()
    {
        _cache = null;
        try
        {
            await js.InvokeVoidAsync("localStorage.removeItem", Chave);
        }
        catch (JSDisconnectedException)
        {
            // nada a fazer: o navegador ja foi embora
        }
    }
}
