using CampanhasAPI.Domain.Events;
using CampanhasAPI.Domain.Interfaces.Events;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Messaging;

/// <summary>
/// Publisher provisório da Fase 2: só registra o evento no log.
/// Substituído pelo RabbitMQEventPublisher na Fase 3.
/// </summary>
public class LogEventPublisher(ILogger<LogEventPublisher> logger) : IEventPublisher
{
    public Task PublicarDoacaoRecebidaAsync(DoacaoRecebidaEvent evento, CancellationToken ct = default)
    {
        logger.LogInformation("[evento] DoacaoRecebida {@Evento}", evento);
        return Task.CompletedTask;
    }

    public Task PublicarCampanhaAtualizadaAsync(CampanhaAtualizadaEvent evento, CancellationToken ct = default)
    {
        logger.LogInformation("[evento] CampanhaAtualizada {@Evento}", evento);
        return Task.CompletedTask;
    }
}
