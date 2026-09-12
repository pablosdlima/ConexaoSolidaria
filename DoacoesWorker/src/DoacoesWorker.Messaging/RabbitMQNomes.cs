namespace DoacoesWorker.Messaging;

/// <summary>
/// Nomes de exchange, fila e routing key. Precisam ser idênticos nos dois serviços.
/// </summary>
public static class RabbitMQNomes
{
    public const string ExchangeDoacoes = "doacoes-exchange";
    public const string FilaDoacoesRecebidas = "doacoes-recebidas-queue";
    public const string FilaDoacoesDlq = "doacoes-recebidas-dlq";
    public const string RoutingKeyDoacaoRecebida = "doacao.recebida";
}
