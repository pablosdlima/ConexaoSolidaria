using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Events;
using CampanhasAPI.Domain.Interfaces.Events;
using CampanhasAPI.Domain.Interfaces.Services;

namespace CampanhasAPI.Application.AppServices;

/// <summary>
/// Orquestra: chama o domain service e publica o evento para o Worker.
/// As regras de negocio ficam no CampanhaService e na entidade.
/// </summary>
public class CampanhaAppService(
    ICampanhaService campanhaService,
    IEventPublisher publisher) : ICampanhaAppService
{
    public async Task<IReadOnlyList<CampanhaResponse>> ListarAsync(CancellationToken ct = default) =>
        (await campanhaService.ListarAsync(ct)).Select(CampanhaResponse.De).ToList();

    public async Task<CampanhaResponse> ObterAsync(Guid id, CancellationToken ct = default) =>
        CampanhaResponse.De(await campanhaService.ObterPorIdAsync(id, ct));

    public async Task<CampanhaResponse> CriarAsync(CampanhaRequest r, CancellationToken ct = default)
    {
        var campanha = await campanhaService.CriarAsync(r.Titulo, r.Descricao, r.DataInicio, r.DataFim, r.MetaFinanceira, ct);
        await PublicarAsync(campanha, ct);
        return CampanhaResponse.De(campanha);
    }

    public async Task<CampanhaResponse> AtualizarAsync(Guid id, CampanhaRequest r, CancellationToken ct = default)
    {
        var campanha = await campanhaService.EditarAsync(id, r.Titulo, r.Descricao, r.DataInicio, r.DataFim, r.MetaFinanceira, ct);
        await PublicarAsync(campanha, ct);
        return CampanhaResponse.De(campanha);
    }

    public async Task<CampanhaResponse> AlterarStatusAsync(Guid id, AlterarStatusCampanhaRequest r, CancellationToken ct = default)
    {
        var campanha = await campanhaService.AlterarStatusAsync(id, r.Status, ct);
        await PublicarAsync(campanha, ct);
        return CampanhaResponse.De(campanha);
    }

    /// <summary>ADR 003: o Worker precisa de titulo, meta e status para montar o painel publico.</summary>
    private Task PublicarAsync(Campanha c, CancellationToken ct) =>
        publisher.PublicarCampanhaAtualizadaAsync(
            new CampanhaAtualizadaEvent(c.Id, c.Titulo, c.MetaFinanceira, c.Status.ToString(), c.DataInicio, c.DataFim), ct);
}
