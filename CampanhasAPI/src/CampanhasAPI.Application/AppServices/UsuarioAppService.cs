using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Application.Dtos.Responses;
using CampanhasAPI.Application.Interfaces;
using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Application.AppServices;

public class UsuarioAppService(
    IUsuarioRepository usuarios,
    ISenhaHasher hasher,
    ILogger<UsuarioAppService> logger) : IUsuarioAppService
{
    public async Task<UsuarioResponse> CadastrarDoadorAsync(CadastrarDoadorRequest request, CancellationToken ct = default)
    {
        if (await usuarios.EmailJaCadastradoAsync(request.Email, ct))
            throw new ConflitoException("Já existe um usuário cadastrado com este e-mail.");

        // RF-3: a senha nunca chega ao banco em texto puro
        var usuario = Usuario.Criar(request.NomeCompleto, request.Email, request.Cpf, hasher.Hash(request.Senha), Role.Doador);
        await usuarios.AdicionarAsync(usuario, ct);

        logger.LogInformation("Doador cadastrado | Id: {Id} | Email: {Email}", usuario.Id, usuario.Email);
        return UsuarioResponse.De(usuario);
    }
}
