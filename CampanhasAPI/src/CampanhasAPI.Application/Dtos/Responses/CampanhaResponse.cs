using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Application.Dtos.Responses;

public sealed record CampanhaResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    DateTimeOffset DataInicio,
    DateTimeOffset DataFim,
    decimal MetaFinanceira,
    string Status,
    DateTimeOffset DataCriacao,
    DateTimeOffset? DataAtualizacao)
{
    public static CampanhaResponse De(Campanha c) => new(
        c.Id, c.Titulo, c.Descricao, c.DataInicio, c.DataFim, c.MetaFinanceira,
        c.Status.ToString(), c.DataCriacao, c.DataAtualizacao);
}
