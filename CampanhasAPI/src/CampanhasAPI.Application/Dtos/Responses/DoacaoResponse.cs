using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Application.Dtos.Responses;

public sealed record DoacaoResponse(Guid Id, Guid IdCampanha, decimal Valor, DateTimeOffset DataHora, string Status)
{
    public static DoacaoResponse De(Doacao d) => new(d.Id, d.IdCampanha, d.Valor, d.DataHora, d.Status.ToString());
}
