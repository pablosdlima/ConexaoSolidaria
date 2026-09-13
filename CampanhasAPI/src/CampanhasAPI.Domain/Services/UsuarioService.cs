using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Interfaces.Repositories;
using CampanhasAPI.Domain.Interfaces.Services;
using CampanhasAPI.Domain.Services.Generic;
using CampanhasAPI.Domain.Validators;
using Microsoft.Extensions.Logging;

namespace CampanhasAPI.Domain.Services;

public class UsuarioService(
    IUsuarioRepository usuarios,
    ISenhaHasher hasher,
    ILogger<UsuarioService> logger) : GenericService<Usuario>(usuarios), IUsuarioService
{
    public async Task<Usuario> CadastrarDoadorAsync(string nomeCompleto, string email, string cpf, string senha, CancellationToken ct = default)
    {
        if (await usuarios.EmailJaCadastradoAsync(email, ct))
            throw new ConflitoException("Já existe um usuário cadastrado com este e-mail.");

        if (await usuarios.CpfJaCadastradoAsync(CpfValidator.Normalizar(cpf), ct))
            throw new ConflitoException("Já existe um usuário cadastrado com este CPF.");

        // RF-3: a senha nunca chega ao banco em texto puro
        var usuario = Usuario.Criar(nomeCompleto, email, cpf, hasher.Hash(senha), Role.Doador);
        await AdicionarAsync(usuario, ct);

        logger.LogInformation("Doador cadastrado | Id: {Id} | Email: {Email}", usuario.Id, usuario.Email);
        return usuario;
    }

    public async Task<Usuario> ValidarLoginAsync(string email, string senha, CancellationToken ct = default)
    {
        var usuario = await usuarios.ObterPorEmailAsync(email, ct);

        // Mesma mensagem para e-mail inexistente e senha errada: nao revela quais e-mails existem
        if (usuario is null || !hasher.Verificar(senha, usuario.SenhaHash))
        {
            logger.LogWarning("Login recusado | Email: {Email}", email);
            throw new AutenticacaoException("E-mail ou senha inválidos.");
        }

        return usuario;
    }
}
