using DoacoesWorker.Application.Consumers;
using DoacoesWorker.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DoacoesWorker.Messaging;

public class CampanhaAtualizadaRabbitConsumer(
    RabbitMQConexao conexao,
    IServiceProvider sp,
    ILogger<CampanhaAtualizadaRabbitConsumer> logger)
    : RabbitMQConsumerBase<CampanhaAtualizadaEvent, CampanhaAtualizadaConsumer>(conexao, sp, logger)
{
    protected override string Fila => RabbitMQNomes.FilaCampanhasAtualizadas;

    protected override Task ProcessarAsync(CampanhaAtualizadaConsumer handler, CampanhaAtualizadaEvent evento, CancellationToken ct) =>
        handler.ProcessAsync(evento, ct);
}
