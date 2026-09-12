using DoacoesWorker.Application.Dtos;
using DoacoesWorker.Application.Interfaces;
using DoacoesWorker.Domain.Interfaces.Services;

namespace DoacoesWorker.Application.AppServices;

public class PainelAppService(IResumoCampanhaService resumo) : IPainelAppService
{
    public async Task<IReadOnlyList<CampanhaPublicaResponse>> ListarCampanhasAtivasAsync(CancellationToken ct = default) =>
        (await resumo.ListarAtivasAsync(ct)).Select(CampanhaPublicaResponse.De).ToList();
}
