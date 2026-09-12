using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Domain.Services;

public class CampanhaService(
    ICampanhaRepository campanhas,
    ILogger<CampanhaService> logger) : ICampanhaService
{
    public Task<IReadOnlyList<Campanha>> ListarAsync(CancellationToken ct = default) =>
        campanhas.ListarAsync(ct);

    public async Task<Campanha> ObterAsync(Guid id, CancellationToken ct = default) =>
        await campanhas.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Campanha {id} não encontrada.");

    public async Task<Campanha> CriarAsync(string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira, CancellationToken ct = default)
    {
        // RF-2: as regras (data no passado, meta > 0) sao validadas dentro de Campanha.Criar
        var campanha = Campanha.Criar(titulo, descricao, dataInicio, dataFim, metaFinanceira);
        await campanhas.AdicionarAsync(campanha, ct);

        logger.LogInformation("Campanha criada | Id: {Id} | Titulo: {Titulo}", campanha.Id, campanha.Titulo);
        return campanha;
    }

    public async Task<Campanha> AtualizarAsync(Guid id, string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira, CancellationToken ct = default)
    {
        var campanha = await ObterAsync(id, ct);
        campanha.Atualizar(titulo, descricao, dataInicio, dataFim, metaFinanceira);
        await campanhas.AtualizarAsync(campanha, ct);

        logger.LogInformation("Campanha atualizada | Id: {Id}", id);
        return campanha;
    }

    public async Task<Campanha> AlterarStatusAsync(Guid id, StatusCampanha novoStatus, CancellationToken ct = default)
    {
        var campanha = await ObterAsync(id, ct);

        switch (novoStatus)
        {
            case StatusCampanha.Concluida: campanha.Concluir(); break;
            case StatusCampanha.Cancelada: campanha.Cancelar(); break;
            default: throw new DomainException("Status deve ser Concluida ou Cancelada.");
        }

        await campanhas.AtualizarAsync(campanha, ct);

        logger.LogInformation("Campanha {Id} agora esta {Status}", id, campanha.Status);
        return campanha;
    }
}
