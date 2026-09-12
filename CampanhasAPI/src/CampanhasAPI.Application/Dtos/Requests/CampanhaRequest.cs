namespace CampanhasAPI.Application.Dtos.Requests;

/// <summary>Usado tanto para criar quanto para editar (PUT) uma campanha.</summary>
public sealed record CampanhaRequest(
    string Titulo,
    string Descricao,
    DateTimeOffset DataInicio,
    DateTimeOffset DataFim,
    decimal MetaFinanceira);
