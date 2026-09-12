using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;

namespace CampanhasAPI.Application.Interfaces;

public interface IAuthAppService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
