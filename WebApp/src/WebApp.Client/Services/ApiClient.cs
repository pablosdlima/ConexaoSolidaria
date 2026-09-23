using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WebApp.Client.Models;

namespace WebApp.Client.Services;

/// <summary>
/// Unico ponto que fala HTTP com o back-end (pelo API Gateway).
///
/// Responsabilidades: anexar o Bearer token, desserializar, e traduzir erro HTTP
/// em ResultadoApi com mensagem legivel. As paginas nao lidam com HttpClient.
/// </summary>
public class ApiClient(HttpClient http, TokenStore tokens)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    // --- Autenticacao e cadastro (publicos) ---
    public Task<ResultadoApi<LoginResponse>> LoginAsync(LoginRequest req) =>
        EnviarAsync<LoginResponse>(HttpMethod.Post, "api/auth/login", req, autenticado: false);

    public Task<ResultadoApi<UsuarioResponse>> CadastrarDoadorAsync(CadastrarDoadorRequest req) =>
        EnviarAsync<UsuarioResponse>(HttpMethod.Post, "api/doadores", req, autenticado: false);

    // --- Painel publico (RF-4, servido pelo DoacoesWorker) ---
    public Task<ResultadoApi<List<CampanhaPublicaResponse>>> PainelPublicoAsync() =>
        EnviarAsync<List<CampanhaPublicaResponse>>(HttpMethod.Get, "api/publico/campanhas", null, autenticado: false);

    // --- Campanhas (RF-2, exige role GestorONG) ---
    public Task<ResultadoApi<List<CampanhaResponse>>> ListarCampanhasAsync() =>
        EnviarAsync<List<CampanhaResponse>>(HttpMethod.Get, "api/campanhas", null);

    public Task<ResultadoApi<CampanhaResponse>> ObterCampanhaAsync(Guid id) =>
        EnviarAsync<CampanhaResponse>(HttpMethod.Get, "api/campanhas/" + id, null);

    public Task<ResultadoApi<CampanhaResponse>> CriarCampanhaAsync(CampanhaRequest req) =>
        EnviarAsync<CampanhaResponse>(HttpMethod.Post, "api/campanhas", req);

    public Task<ResultadoApi<CampanhaResponse>> EditarCampanhaAsync(Guid id, CampanhaRequest req) =>
        EnviarAsync<CampanhaResponse>(HttpMethod.Put, "api/campanhas/" + id, req);

    public Task<ResultadoApi<CampanhaResponse>> AlterarStatusAsync(Guid id, string status) =>
        EnviarAsync<CampanhaResponse>(HttpMethod.Patch, "api/campanhas/" + id + "/status",
            new AlterarStatusCampanhaRequest(status));

    // --- Doacoes (RF-5, exige role Doador) ---
    public Task<ResultadoApi<DoacaoResponse>> DoarAsync(DoacaoRequest req) =>
        EnviarAsync<DoacaoResponse>(HttpMethod.Post, "api/doacoes", req);

    public Task<ResultadoApi<List<DoacaoResponse>>> MinhasDoacoesAsync() =>
        EnviarAsync<List<DoacaoResponse>>(HttpMethod.Get, "api/doacoes/minhas", null);

    // --- Motor ---
    private async Task<ResultadoApi<T>> EnviarAsync<T>(HttpMethod metodo, string rota, object? corpo, bool autenticado = true)
    {
        using var req = new HttpRequestMessage(metodo, rota);
        if (corpo is not null) req.Content = JsonContent.Create(corpo, options: Json);

        if (autenticado)
        {
            var token = await tokens.ObterAsync();
            if (!string.IsNullOrWhiteSpace(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        HttpResponseMessage resp;
        try
        {
            resp = await http.SendAsync(req);
        }
        catch (Exception ex)
        {
            // API fora do ar ou CORS bloqueado — o navegador nao detalha, por seguranca.
            return ResultadoApi<T>.Falha("Nao foi possivel falar com a API: " + ex.Message, 0);
        }

        if (resp.IsSuccessStatusCode)
        {
            if (resp.StatusCode == HttpStatusCode.NoContent)
                return ResultadoApi<T>.Ok(default!, (int)resp.StatusCode);

            var dados = await resp.Content.ReadFromJsonAsync<T>(Json);
            return ResultadoApi<T>.Ok(dados!, (int)resp.StatusCode);
        }

        return ResultadoApi<T>.Falha(await MensagemDeErroAsync(resp), (int)resp.StatusCode);
    }

    /// <summary>Extrai a mensagem do ProblemDetails; cai para um texto por status quando nao houver.</summary>
    private static async Task<string> MensagemDeErroAsync(HttpResponseMessage resp)
    {
        try
        {
            var problema = await resp.Content.ReadFromJsonAsync<ProblemDetailsResponse>(Json);

            if (problema?.Errors is { Count: > 0 })
                return string.Join(" ", problema.Errors.SelectMany(e => e.Value));

            if (!string.IsNullOrWhiteSpace(problema?.Detail))
                return problema.Detail;
        }
        catch
        {
            // corpo nao era ProblemDetails
        }

        return resp.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Sessao expirada ou credenciais invalidas.",
            HttpStatusCode.Forbidden => "Voce nao tem permissao para esta acao.",
            HttpStatusCode.NotFound => "Recurso nao encontrado.",
            HttpStatusCode.Conflict => "Ja existe um registro com estes dados.",
            (HttpStatusCode)429 => "Muitas tentativas. Aguarde um minuto.",
            _ => "Erro inesperado (" + (int)resp.StatusCode + ")."
        };
    }
}
