namespace WebApp.Client.Services;

/// <summary>
/// Resultado de uma chamada a API sem lancar excecao para erro esperado
/// (401, 403, 409, 400 de validacao). A pagina decide o que mostrar.
/// </summary>
public sealed class ResultadoApi<T>
{
    public bool Sucesso { get; init; }
    public T? Dados { get; init; }
    public string? Erro { get; init; }
    public int StatusCode { get; init; }

    public static ResultadoApi<T> Ok(T dados, int status = 200) =>
        new() { Sucesso = true, Dados = dados, StatusCode = status };

    public static ResultadoApi<T> Falha(string erro, int status) =>
        new() { Sucesso = false, Erro = erro, StatusCode = status };
}
