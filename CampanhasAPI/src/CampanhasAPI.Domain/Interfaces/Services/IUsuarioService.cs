using CampanhasAPI.Domain.Entities;

namespace CampanhasAPI.Domain.Interfaces.Services;

public interface IUsuarioService
{
    /// <summary>RF-3: e-mail único, CPF válido, senha com hash. Lança ConflitoException se o e-mail já existe.</summary>
    Task<Usuario> CadastrarDoadorAsync(string nomeCompleto, string email, string cpf, string senha, CancellationToken ct = default);

    /// <summary>Lança AutenticacaoException se e-mail ou senha não conferem.</summary>
    Task<Usuario> ValidarLoginAsync(string email, string senha, CancellationToken ct = default);
}
