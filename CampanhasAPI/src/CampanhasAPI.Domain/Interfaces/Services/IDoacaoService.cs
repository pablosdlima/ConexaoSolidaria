using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Domain.Interfaces.Services;

public interface IDoacaoService
{
    /// <summary>
    /// RF-5: registra a intenção de doação como Pendente. Lança NotFoundException se a
    /// campanha não existe e DomainException se ela não aceita doações.
    /// </summary>
    Task<Doacao> RegistrarAsync(Guid idCampanha, Guid idDoador, decimal valor, CancellationToken ct = default);

    Task MarcarPublicadaAsync(Doacao doacao, CancellationToken ct = default);
    Task<IReadOnlyList<Doacao>> ListarDoDoadorAsync(Guid idDoador, CancellationToken ct = default);
}
