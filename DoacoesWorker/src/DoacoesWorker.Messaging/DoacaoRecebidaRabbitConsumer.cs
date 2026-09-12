using DoacoesWorker.Application.Consumers;
using DoacoesWorker.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DoacoesWorker.Messaging;

public class DoacaoRecebidaRabbitConsumer(
    RabbitMQConexao conexao,
    IServiceProvider sp,
    ILogger<DoacaoRecebidaRabbitConsumer> logger)
    : RabbitMQConsumerBase<DoacaoRecebidaEvent, DoacaoRecebidaConsumer>(conexao, sp, logger)
{
    protected override string Fila => RabbitMQNomes.FilaDoacoesRecebidas;

    protected override Task ProcessarAsync(DoacaoRecebidaConsumer handler, DoacaoRecebidaEvent evento, CancellationToken ct) =>
        handler.ProcessAsync(evento, ct);
}
