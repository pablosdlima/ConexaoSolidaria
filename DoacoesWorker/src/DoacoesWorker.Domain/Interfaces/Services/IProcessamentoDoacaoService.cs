using DoacoesWorker.Domain.Events;

namespace DoacoesWorker.Domain.Interfaces.Services;

public interface IProcessamentoDoacaoService
{
    /// <summary>Soma a doacao no total da campanha uma unica vez. Retorna false se ja tinha sido processada.</summary>
    Task<bool> ProcessarAsync(DoacaoRecebidaEvent evento, CancellationToken ct = default);
}
