namespace CampanhasAPI.Domain.Interfaces.Services;

/// <summary>Metricas de negocio. A implementacao (Prometheus) fica fora do Domain.</summary>
public interface IMetricas
{
    void DoacaoRegistrada(decimal valor);
    void CampanhaCriada();
    void EventoPublicado(string tipo);
}
