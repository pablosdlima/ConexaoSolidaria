using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Events;
using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Interfaces.Events;
using CampanhasAPI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Application.AppServices;

public class CampanhaAppService(
    ICampanhaRepository campanhas,
    IEventPublisher publisher,
    ILogger<CampanhaAppService> logger) : ICampanhaAppService
{
    public async Task<IReadOnlyList<CampanhaResponse>> ListarAsync(CancellationToken ct = default) =>
        (await campanhas.ListarAsync(ct)).Select(CampanhaResponse.De).ToList();

    public async Task<CampanhaResponse> ObterAsync(Guid id, CancellationToken ct = default) =>
        CampanhaResponse.De(await ObterOuFalhar(id, ct));

    public async Task<CampanhaResponse> CriarAsync(CampanhaRequest r, CancellationToken ct = default)
    {
        var campanha = Campanha.Criar(r.Titulo, r.Descricao, r.DataInicio, r.DataFim, r.MetaFinanceira);
        await campanhas.AdicionarAsync(campanha, ct);
        await PublicarAsync(campanha, ct);

        logger.LogInformation("Campanha criada | Id: {Id} | Titulo: {Titulo}", campanha.Id, campanha.Titulo);
        return CampanhaResponse.De(campanha);
    }

    public async Task<CampanhaResponse> AtualizarAsync(Guid id, CampanhaRequest r, CancellationToken ct = default)
    {
        var campanha = await ObterOuFalhar(id, ct);
        campanha.Atualizar(r.Titulo, r.Descricao, r.DataInicio, r.DataFim, r.MetaFinanceira);
        await campanhas.AtualizarAsync(campanha, ct);
        await PublicarAsync(campanha, ct);

        logger.LogInformation("Campanha atualizada | Id: {Id}", id);
        return CampanhaResponse.De(campanha);
    }

    public async Task<CampanhaResponse> AlterarStatusAsync(Guid id, AlterarStatusCampanhaRequest r, CancellationToken ct = default)
    {
        var campanha = await ObterOuFalhar(id, ct);

        if (r.Status == StatusCampanha.Concluida) campanha.Concluir();
        else if (r.Status == StatusCampanha.Cancelada) campanha.Cancelar();
        else throw new DomainException("Status deve ser Concluida ou Cancelada.");

        await campanhas.AtualizarAsync(campanha, ct);
        await PublicarAsync(campanha, ct);

        logger.LogInformation("Campanha {Id} agora esta {Status}", id, campanha.Status);
        return CampanhaResponse.De(campanha);
    }

    private async Task<Campanha> ObterOuFalhar(Guid id, CancellationToken ct) =>
        await campanhas.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Campanha {id} nao encontrada.");

    /// <summary>ADR 003: o Worker precisa de titulo, meta e status para montar o painel publico.</summary>
    private Task PublicarAsync(Campanha c, CancellationToken ct) =>
        publisher.PublicarCampanhaAtualizadaAsync(
            new CampanhaAtualizadaEvent(c.Id, c.Titulo, c.MetaFinanceira, c.Status.ToString(), c.DataInicio, c.DataFim), ct);
}
