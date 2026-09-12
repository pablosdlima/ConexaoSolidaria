using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Domain.Interfaces.Services;

namespace CampanhasAPI.Application.AppServices;

public class UsuarioAppService(IUsuarioService usuarioService) : IUsuarioAppService
{
    public async Task<UsuarioResponse> CadastrarDoadorAsync(CadastrarDoadorRequest r, CancellationToken ct = default)
    {
        var usuario = await usuarioService.CadastrarDoadorAsync(r.NomeCompleto, r.Email, r.Cpf, r.Senha, ct);
        return UsuarioResponse.De(usuario);
    }
}
