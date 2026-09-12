using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Application.AppServices;

public class AuthAppService(
    IUsuarioService usuarioService,
    IJwtGenerator jwt,
    ILogger<AuthAppService> logger) : IAuthAppService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest r, CancellationToken ct = default)
    {
        var usuario = await usuarioService.ValidarLoginAsync(r.Email, r.Senha, ct);
        var (token, expiraEm) = jwt.Gerar(usuario);

        logger.LogInformation("Login ok | Id: {Id} | Role: {Role}", usuario.Id, usuario.Role);
        return new LoginResponse(token, expiraEm, usuario.Role.ToString());
    }
}
