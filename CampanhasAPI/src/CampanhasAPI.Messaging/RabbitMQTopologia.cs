using RabbitMQ.Client;

namespace CampanhasAPI.Messaging;

/// <summary>
/// Declara exchange, filas, DLX e bindings. Idempotente: os dois serviços chamam
/// no startup, e quem subir primeiro cria — o outro só confirma que existe.
/// </summary>
public static class RabbitMQTopologia
{
    public static async Task DeclararAsync(IChannel canal, CancellationToken ct = default)
    {
        await canal.ExchangeDeclareAsync(RabbitMQNomes.Exchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: ct);
        await canal.ExchangeDeclareAsync(RabbitMQNomes.ExchangeDlx, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: ct);

        await DeclararFilaComDlqAsync(canal, RabbitMQNomes.FilaDoacoesRecebidas, RabbitMQNomes.FilaDoacoesRecebidasDlq, RabbitMQNomes.RoutingKeyDoacaoRecebida, ct);
        await DeclararFilaComDlqAsync(canal, RabbitMQNomes.FilaCampanhasAtualizadas, RabbitMQNomes.FilaCampanhasAtualizadasDlq, RabbitMQNomes.RoutingKeyCampanhaAtualizada, ct);
    }

    private static async Task DeclararFilaComDlqAsync(IChannel canal, string fila, string dlq, string routingKey, CancellationToken ct)
    {
        // DLQ primeiro, para a fila principal poder apontar para ela
        await canal.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await canal.QueueBindAsync(dlq, RabbitMQNomes.ExchangeDlx, routingKey, cancellationToken: ct);

        var args = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = RabbitMQNomes.ExchangeDlx
        };
        await canal.QueueDeclareAsync(fila, durable: true, exclusive: false, autoDelete: false, arguments: args, cancellationToken: ct);
        await canal.QueueBindAsync(fila, RabbitMQNomes.Exchange, routingKey, cancellationToken: ct);
    }
}
