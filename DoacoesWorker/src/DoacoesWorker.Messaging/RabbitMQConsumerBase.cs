using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DoacoesWorker.Messaging;

/// <summary>
/// Consumer generico (adaptado do RabbitMQConsumer da Fase 3):
///   - prefetch 1, ACK manual so depois do handler terminar;
///   - handler resolvido num scope por mensagem;
///   - excecao => NACK sem requeue => a mensagem vai para a DLQ da fila.
/// Cada fila tem uma subclasse que diz qual evento e qual handler usar.
/// </summary>
public abstract class RabbitMQConsumerBase<TEvento, THandler>(
    RabbitMQConexao conexao,
    IServiceProvider serviceProvider,
    ILogger logger) : BackgroundService
    where THandler : class
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private IChannel? _canal;

    protected abstract string Fila { get; }
    protected abstract Task ProcessarAsync(THandler handler, TEvento evento, CancellationToken ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var conn = await conexao.ObterAsync(stoppingToken);
        _canal = await conn.CreateChannelAsync(cancellationToken: stoppingToken);
        await _canal.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_canal);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var messageId = ea.BasicProperties.MessageId ?? "(sem id)";
            try
            {
                var evento = JsonSerializer.Deserialize<TEvento>(ea.Body.Span, Json)
                    ?? throw new InvalidOperationException("Corpo da mensagem vazio.");

                logger.LogInformation("Mensagem recebida | Fila: {Fila} | MessageId: {MessageId}", Fila, messageId);

                using var scope = serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<THandler>();
                await ProcessarAsync(handler, evento, stoppingToken);

                await _canal.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao processar | Fila: {Fila} | MessageId: {MessageId} — enviando para a DLQ", Fila, messageId);
                await _canal.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        await _canal.BasicConsumeAsync(Fila, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Consumer iniciado | Fila: {Fila}", Fila);

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_canal is not null)
        {
            await _canal.CloseAsync(cancellationToken);
            _canal.Dispose();
        }
        await base.StopAsync(cancellationToken);
    }
}
