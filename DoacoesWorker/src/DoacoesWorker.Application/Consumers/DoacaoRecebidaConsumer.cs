using DoacoesWorker.Domain.Events;
using DoacoesWorker.Domain.Interfaces.Services;

namespace DoacoesWorker.Application.Consumers;

/// <summary>Handler da fila doacoes-recebidas. Scoped: uma instancia por mensagem (padrao da Fase 3).</summary>
public class DoacaoRecebidaConsumer(IProcessamentoDoacaoService processamento)
{
    public Task ProcessAsync(DoacaoRecebidaEvent evento, CancellationToken ct) =>
        processamento.ProcessarAsync(evento, ct);
}
