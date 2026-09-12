using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Application.AppServices;

public class AuthAppService(
    IUsuarioRepository usuarios,
    ISenhaHasher hasher,
    IJwtGenerator jwt,
    ILogger<AuthAppService> logger) : IAuthAppService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await usuarios.ObterPorEmailAsync(request.Email, ct);

        // Mesma mensagem para e-mail inexistente e senha errada: nao revela quais e-mails existem
        if (usuario is null || !hasher.Verificar(request.Senha, usuario.SenhaHash))
        {
            logger.LogWarning("Login recusado | Email: {Email}", request.Email);
            throw new AutenticacaoException("E-mail ou senha inválidos.");
        }

        var (token, expiraEm) = jwt.Gerar(usuario);
        logger.LogInformation("Login ok | Id: {Id} | Role: {Role}", usuario.Id, usuario.Role);
        return new LoginResponse(token, expiraEm, usuario.Role.ToString());
    }
}
