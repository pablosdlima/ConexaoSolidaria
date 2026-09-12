namespace CampanhasAPI.Domain.Events;

/// <summary>
/// Publicado quando uma campanha é criada, editada ou muda de status.
/// O DoacoesWorker usa para manter o resumo que alimenta o painel público
/// (título, meta, status) sem precisar consultar o banco da API.
/// </summary>
public sealed record CampanhaAtualizadaEvent(
    Guid IdCampanha,
    string Titulo,
    decimal MetaFinanceira,
    string Status,
    DateTimeOffset DataInicio,
    DateTimeOffset DataFim);
