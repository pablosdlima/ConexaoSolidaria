using Microsoft.Extensions.Logging;

namespace DoacoesWorker.Messaging;

public class RabbitMQInitializer(RabbitMQConexao conexao, ILogger<RabbitMQInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var conn = await conexao.ObterAsync(ct);
        await using var canal = await conn.CreateChannelAsync(cancellationToken: ct);
        await RabbitMQTopologia.DeclararAsync(canal, ct);

        logger.LogInformation("RabbitMQ pronto | Exchange: {Exchange} | Consumindo: {Doacoes}, {Campanhas}",
            RabbitMQNomes.Exchange, RabbitMQNomes.FilaDoacoesRecebidas, RabbitMQNomes.FilaCampanhasAtualizadas);
    }
}
