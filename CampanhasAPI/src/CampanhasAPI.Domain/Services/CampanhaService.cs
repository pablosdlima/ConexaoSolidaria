using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using CampanhasAPI.Domain.Services.Generic;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Domain.Services;

public class CampanhaService(
    ICampanhaRepository campanhas,
    IMetricas metricas,
    ILogger<CampanhaService> logger) : GenericService<Campanha>(campanhas), ICampanhaService
{
    public Task<IReadOnlyList<Campanha>> ListarAsync(CancellationToken ct = default) =>
        campanhas.ListarAsync(ct);

    public async Task<Campanha> CriarAsync(string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira, CancellationToken ct = default)
    {
        // RF-2: as regras (data no passado, meta > 0) sao validadas dentro de Campanha.Criar
        var campanha = Campanha.Criar(titulo, descricao, dataInicio, dataFim, metaFinanceira);
        await AdicionarAsync(campanha, ct);

        metricas.CampanhaCriada();
        logger.LogInformation("Campanha criada | Id: {Id} | Titulo: {Titulo}", campanha.Id, campanha.Titulo);
        return campanha;
    }

    public async Task<Campanha> EditarAsync(Guid id, string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira, CancellationToken ct = default)
    {
        var campanha = await ObterPorIdAsync(id, ct);
        campanha.Atualizar(titulo, descricao, dataInicio, dataFim, metaFinanceira);
        await AtualizarAsync(campanha, ct);

        logger.LogInformation("Campanha editada | Id: {Id}", id);
        return campanha;
    }

    public async Task<Campanha> AlterarStatusAsync(Guid id, StatusCampanha novoStatus, CancellationToken ct = default)
    {
        var campanha = await ObterPorIdAsync(id, ct);
        campanha.AlterarStatus(novoStatus);   // a regra de transicao mora na entidade
        await AtualizarAsync(campanha, ct);

        logger.LogInformation("Campanha {Id} agora esta {Status}", id, campanha.Status);
        return campanha;
    }
}
