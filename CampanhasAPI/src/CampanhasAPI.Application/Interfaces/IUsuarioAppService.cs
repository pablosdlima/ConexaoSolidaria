using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;

namespace CampanhasAPI.Application.Interfaces;

public interface IUsuarioAppService
{
    Task<UsuarioResponse> CadastrarDoadorAsync(CadastrarDoadorRequest request, CancellationToken ct = default);
}
