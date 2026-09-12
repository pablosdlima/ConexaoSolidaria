namespace DoacoesWorker.Messaging;

/// <summary>
/// Topologia do RabbitMQ. Este arquivo é idêntico nos dois serviços — os nomes
/// são o contrato entre eles, junto com o JSON dos eventos.
///
///   conexao-solidaria (direct)
///     ├─ doacao.recebida      → fila doacoes-recebidas       → DLQ doacoes-recebidas.dlq
///     └─ campanha.atualizada  → fila campanhas-atualizadas   → DLQ campanhas-atualizadas.dlq
/// </summary>
public static class RabbitMQNomes
{
    public const string Exchange = "conexao-solidaria";
    public const string ExchangeDlx = "conexao-solidaria.dlx";

    public const string RoutingKeyDoacaoRecebida = "doacao.recebida";
    public const string RoutingKeyCampanhaAtualizada = "campanha.atualizada";

    public const string FilaDoacoesRecebidas = "doacoes-recebidas";
    public const string FilaCampanhasAtualizadas = "campanhas-atualizadas";

    public const string FilaDoacoesRecebidasDlq = "doacoes-recebidas.dlq";
    public const string FilaCampanhasAtualizadasDlq = "campanhas-atualizadas.dlq";
}
