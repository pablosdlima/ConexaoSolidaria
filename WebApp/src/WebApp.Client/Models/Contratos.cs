namespace WebApp.Client.Models;

// Espelham os DTOs da CampanhasAPI e do DoacoesWorker. Ficam no projeto .Client
// porque e o WebAssembly (navegador) quem fala com a API.

public sealed record LoginRequest(string Email, string Senha);
public sealed record LoginResponse(string Token, DateTimeOffset ExpiraEm, string Role);

public sealed record CadastrarDoadorRequest(string NomeCompleto, string Email, string Cpf, string Senha);
public sealed record UsuarioResponse(Guid Id, string NomeCompleto, string Email, string Role);

public sealed record CampanhaRequest(
    string Titulo, string Descricao,
    DateTimeOffset DataInicio, DateTimeOffset DataFim,
    decimal MetaFinanceira);

public sealed record CampanhaResponse(
    Guid Id, string Titulo, string Descricao,
    DateTimeOffset DataInicio, DateTimeOffset DataFim,
    decimal MetaFinanceira, string Status,
    DateTimeOffset DataCriacao, DateTimeOffset? DataAtualizacao);

public sealed record AlterarStatusCampanhaRequest(string Status);

public sealed record DoacaoRequest(Guid IdCampanha, decimal ValorDoacao);
public sealed record DoacaoResponse(Guid Id, Guid IdCampanha, decimal Valor, DateTimeOffset DataHora, string Status);

/// <summary>RF-4: o que o painel publico devolve (vem do DoacoesWorker).</summary>
public sealed record CampanhaPublicaResponse(
    Guid IdCampanha, string Titulo,
    decimal MetaFinanceira, decimal ValorTotalArrecadado,
    decimal PercentualAtingido, int QuantidadeDoacoes,
    DateTimeOffset DataFim);

/// <summary>Formato de erro da API (ProblemDetails, RFC 7807).</summary>
public sealed record ProblemDetailsResponse(string? Title, int? Status, string? Detail,
    Dictionary<string, string[]>? Errors);
