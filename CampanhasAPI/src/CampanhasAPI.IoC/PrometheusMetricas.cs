using CampanhasAPI.Domain.Interfaces.Services;
using Prometheus;

namespace CampanhasAPI.IoC;

/// <summary>Singleton: os contadores sao criados uma vez, no startup (campos de instancia, nao static — static seria inicializado so no primeiro uso).</summary>
public class PrometheusMetricas : IMetricas
{
    private readonly Counter DoacoesRegistradas = Metrics.CreateCounter(
        "campanhas_doacoes_registradas_total", "Intencoes de doacao registradas pela API.");

    private readonly Counter ValorRegistrado = Metrics.CreateCounter(
        "campanhas_doacoes_valor_registrado_total", "Soma (R$) das intencoes de doacao registradas.");

    private readonly Counter CampanhasCriadas = Metrics.CreateCounter(
        "campanhas_criadas_total", "Campanhas criadas.");

    private readonly Counter EventosPublicados = Metrics.CreateCounter(
        "campanhas_eventos_publicados_total", "Eventos publicados no RabbitMQ.", "tipo");

    public void DoacaoRegistrada(decimal valor)
    {
        DoacoesRegistradas.Inc();
        ValorRegistrado.Inc((double)valor);
    }

    public void CampanhaCriada() => CampanhasCriadas.Inc();
    public void EventoPublicado(string tipo) => EventosPublicados.WithLabels(tipo).Inc();
}
