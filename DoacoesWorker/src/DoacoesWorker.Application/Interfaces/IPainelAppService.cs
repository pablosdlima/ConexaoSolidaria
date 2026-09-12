using DoacoesWorker.Application.Dtos;

namespace DoacoesWorker.Application.Interfaces;

public interface IPainelAppService
{
    Task<IReadOnlyList<CampanhaPublicaResponse>> ListarCampanhasAtivasAsync(CancellationToken ct = default);
}
