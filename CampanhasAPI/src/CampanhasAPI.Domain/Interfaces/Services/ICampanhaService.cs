using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;

namespace CampanhasAPI.Domain.Interfaces.Services;

public interface ICampanhaService
{
    Task<IReadOnlyList<Campanha>> ListarAsync(CancellationToken ct = default);

    /// <summary>Lança NotFoundException se não existir.</summary>
    Task<Campanha> ObterAsync(Guid id, CancellationToken ct = default);

    Task<Campanha> CriarAsync(string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira, CancellationToken ct = default);
    Task<Campanha> AtualizarAsync(Guid id, string titulo, string descricao, DateTimeOffset dataInicio, DateTimeOffset dataFim, decimal metaFinanceira, CancellationToken ct = default);
    Task<Campanha> AlterarStatusAsync(Guid id, StatusCampanha novoStatus, CancellationToken ct = default);
}
