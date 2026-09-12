using DoacoesWorker.Domain.Events;
using DoacoesWorker.Domain.Interfaces.Services;

namespace DoacoesWorker.Application.Consumers;

/// <summary>Handler da fila campanhas-atualizadas.</summary>
public class CampanhaAtualizadaConsumer(IResumoCampanhaService resumo)
{
    public Task ProcessAsync(CampanhaAtualizadaEvent evento, CancellationToken ct) =>
        resumo.AtualizarAsync(evento, ct);
}
