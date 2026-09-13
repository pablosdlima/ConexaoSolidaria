using System.Text;
using System.Text.Json;
using CampanhasAPI.Domain.Events;
using CampanhasAPI.Domain.Interfaces.Events;
using CampanhasAPI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace CampanhasAPI.Messaging;

public class RabbitMQEventPublisher(RabbitMQConexao conexao, IMetricas metricas, ILogger<RabbitMQEventPublisher> logger) : IEventPublisher
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public Task PublicarDoacaoRecebidaAsync(DoacaoRecebidaEvent evento, CancellationToken ct = default) =>
        PublicarAsync(RabbitMQNomes.RoutingKeyDoacaoRecebida, evento, evento.IdDoacao.ToString(), ct);

    public Task PublicarCampanhaAtualizadaAsync(CampanhaAtualizadaEvent evento, CancellationToken ct = default) =>
        PublicarAsync(RabbitMQNomes.RoutingKeyCampanhaAtualizada, evento, evento.IdCampanha.ToString(), ct);

    private async Task PublicarAsync<T>(string routingKey, T evento, string messageId, CancellationToken ct)
    {
        var conn = await conexao.ObterAsync(ct);
        await using var canal = await conn.CreateChannelAsync(cancellationToken: ct);

        var body = JsonSerializer.SerializeToUtf8Bytes(evento, Json);
        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent, // sobrevive a restart do broker
            MessageId = messageId,
            Type = typeof(T).Name,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await canal.BasicPublishAsync(RabbitMQNomes.Exchange, routingKey, mandatory: false, props, body, ct);

        metricas.EventoPublicado(typeof(T).Name);
        logger.LogInformation("Evento publicado | {Tipo} | RoutingKey: {RoutingKey} | MessageId: {MessageId}",
            typeof(T).Name, routingKey, messageId);
    }
}
