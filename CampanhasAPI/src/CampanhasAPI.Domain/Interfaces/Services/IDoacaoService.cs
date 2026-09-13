using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Generic;

namespace CampanhasAPI.Domain.Interfaces.Services;

public interface IDoacaoService : IGenericService<Doacao>
{
    /// <summary>
    /// RF-5: registra a intencao de doacao como Pendente. Lanca NotFoundException se a
    /// campanha nao existe e DomainException se ela nao aceita doacoes.
    /// </summary>
    Task<Doacao> RegistrarAsync(Guid idCampanha, Guid idDoador, decimal valor, CancellationToken ct = default);

    Task MarcarPublicadaAsync(Doacao doacao, CancellationToken ct = default);
    Task<IReadOnlyList<Doacao>> ListarDoDoadorAsync(Guid idDoador, CancellationToken ct = default);
}
