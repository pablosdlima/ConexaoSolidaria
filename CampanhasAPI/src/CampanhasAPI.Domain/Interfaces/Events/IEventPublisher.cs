using CampanhasAPI.Domain.Events;

namespace CampanhasAPI.Domain.Interfaces.Events;

public interface IEventPublisher
{
    Task PublicarDoacaoRecebidaAsync(DoacaoRecebidaEvent evento, CancellationToken ct = default);
    Task PublicarCampanhaAtualizadaAsync(CampanhaAtualizadaEvent evento, CancellationToken ct = default);
}
