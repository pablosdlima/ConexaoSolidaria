using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;

namespace CampanhasAPI.Application.Interfaces;

public interface ICampanhaAppService
{
    Task<IReadOnlyList<CampanhaResponse>> ListarAsync(CancellationToken ct = default);
    Task<CampanhaResponse> ObterAsync(Guid id, CancellationToken ct = default);
    Task<CampanhaResponse> CriarAsync(CampanhaRequest request, CancellationToken ct = default);
    Task<CampanhaResponse> AtualizarAsync(Guid id, CampanhaRequest request, CancellationToken ct = default);
    Task<CampanhaResponse> AlterarStatusAsync(Guid id, AlterarStatusCampanhaRequest request, CancellationToken ct = default);
}
