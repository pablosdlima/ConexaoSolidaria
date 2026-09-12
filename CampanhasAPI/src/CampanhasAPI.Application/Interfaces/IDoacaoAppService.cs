using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;

namespace CampanhasAPI.Application.Interfaces;

public interface IDoacaoAppService
{
    Task<DoacaoResponse> DoarAsync(Guid idDoador, DoacaoRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<DoacaoResponse>> MinhasDoacoesAsync(Guid idDoador, CancellationToken ct = default);
}
