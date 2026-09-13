using DoacoesWorker.Domain.Interfaces.Services;
using Prometheus;

namespace DoacoesWorker.IoC;

/// <summary>Singleton: os contadores sao criados uma vez, no startup (campos de instancia, nao static — static seria inicializado so no primeiro uso).</summary>
public class PrometheusMetricas : IMetricas
{
    private readonly Counter Processadas = Metrics.CreateCounter(
        "doacoes_processadas_total", "Doacoes somadas ao total da campanha pelo Worker.");

    private readonly Counter ValorProcessado = Metrics.CreateCounter(
        "doacoes_valor_processado_total", "Soma (R$) das doacoes processadas.");

    private readonly Counter Duplicadas = Metrics.CreateCounter(
        "doacoes_duplicadas_ignoradas_total", "Reentregas ignoradas pela idempotencia.");

    private readonly Counter Resumos = Metrics.CreateCounter(
        "doacoes_resumos_atualizados_total", "Eventos CampanhaAtualizada aplicados.");

    private readonly Histogram Duracao = Metrics.CreateHistogram(
        "doacoes_processamento_duracao_segundos", "Tempo para processar uma doacao (ledger + $inc).",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.005, 2, 10) });

    public void DoacaoProcessada(decimal valor)
    {
        Processadas.Inc();
        ValorProcessado.Inc((double)valor);
    }

    public void DoacaoDuplicadaIgnorada() => Duplicadas.Inc();
    public void ResumoAtualizado() => Resumos.Inc();
    public IDisposable MedirProcessamento() => Duracao.NewTimer();
}
