using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Interfaces.Generic;

namespace CampanhasAPI.Domain.Interfaces.Services;

public interface ICampanhaService : IGenericService<Campanha>
{
    Task<IReadOnlyList<Campanha>> ListarAsync(CancellationToken ct = default);
    Task<Campanha> CriarAsync(string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira, CancellationToken ct = default);
    Task<Campanha> EditarAsync(Guid id, string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira, CancellationToken ct = default);
    Task<Campanha> AlterarStatusAsync(Guid id, StatusCampanha novoStatus, CancellationToken ct = default);
}
