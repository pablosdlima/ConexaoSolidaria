namespace DoacoesWorker.Domain.Events;

/// <summary>
/// Copia do contrato publicado pela CampanhasAPI (mesmo JSON). Mantem o resumo
/// da campanha atualizado sem o Worker precisar consultar o banco da API.
/// </summary>
public sealed record CampanhaAtualizadaEvent(
    Guid IdCampanha,
    string Titulo,
    decimal MetaFinanceira,
    string Status,
    DateTimeOffset DataInicio,
    DateTimeOffset DataFim);
