using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Domain.Interfaces.Events;
using CampanhasAPI.Domain.Interfaces.Services;

namespace CampanhasAPI.Application.AppServices;

/// <summary>
/// RT-Mensageria: a API registra a doacao e publica DoacaoRecebidaEvent.
/// Ela NAO soma nada na campanha — isso e trabalho do DoacoesWorker.
/// </summary>
public class DoacaoAppService(
    IDoacaoService doacaoService,
    IEventPublisher publisher) : IDoacaoAppService
{
    public async Task<DoacaoResponse> DoarAsync(Guid idDoador, DoacaoRequest r, CancellationToken ct = default)
    {
        var doacao = await doacaoService.RegistrarAsync(r.IdCampanha, idDoador, r.ValorDoacao, ct);

        await publisher.PublicarDoacaoRecebidaAsync(doacao.ParaEvento(), ct);
        await doacaoService.MarcarPublicadaAsync(doacao, ct);

        return DoacaoResponse.De(doacao);
    }

    public async Task<IReadOnlyList<DoacaoResponse>> MinhasDoacoesAsync(Guid idDoador, CancellationToken ct = default) =>
        (await doacaoService.ListarDoDoadorAsync(idDoador, ct)).Select(DoacaoResponse.De).ToList();
}
