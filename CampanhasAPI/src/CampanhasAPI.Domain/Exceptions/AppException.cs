namespace CampanhasAPI.Domain.Exceptions;

/// <summary>
/// Base das excecoes que viram resposta HTTP. Cada subclasse declara o proprio
/// status e titulo — o middleware trata todas do mesmo jeito e nao precisa mudar
/// quando uma excecao nova aparece (OCP).
/// </summary>
public abstract class AppException(int statusCode, string titulo, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Titulo { get; } = titulo;
}
