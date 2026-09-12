using DoacoesWorker.Domain.Entities;

namespace DoacoesWorker.Application.Dtos;

/// <summary>RF-4: Titulo, MetaFinanceira e ValorTotalArrecadado. So campanhas Ativa.</summary>
public sealed record CampanhaPublicaResponse(
    Guid IdCampanha,
    string Titulo,
    decimal MetaFinanceira,
    decimal ValorTotalArrecadado,
    decimal PercentualAtingido,
    int QuantidadeDoacoes,
    DateTimeOffset DataFim)
{
    public static CampanhaPublicaResponse De(CampanhaResumo r) => new(
        r.IdCampanha,
        r.Titulo,
        r.MetaFinanceira,
        r.ValorTotalArrecadado,
        r.MetaFinanceira > 0 ? Math.Round(r.ValorTotalArrecadado / r.MetaFinanceira * 100, 2) : 0,
        r.QuantidadeDoacoes,
        r.DataFim);
}
