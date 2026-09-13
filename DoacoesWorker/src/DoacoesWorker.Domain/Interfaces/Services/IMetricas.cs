namespace DoacoesWorker.Domain.Interfaces.Services;

/// <summary>Metricas de negocio. A implementacao (Prometheus) fica fora do Domain.</summary>
public interface IMetricas
{
    void DoacaoProcessada(decimal valor);
    void DoacaoDuplicadaIgnorada();
    void ResumoAtualizado();
    IDisposable MedirProcessamento();
}
