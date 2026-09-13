using CampanhasAPI.Domain.Entities;
using CampanhasAPI.Domain.Interfaces.Generic;

namespace CampanhasAPI.Domain.Interfaces.Services;

public interface IUsuarioService : IGenericService<Usuario>
{
    /// <summary>RF-3: e-mail e CPF unicos, senha com hash. Lanca ConflitoException se ja existir.</summary>
    Task<Usuario> CadastrarDoadorAsync(string nomeCompleto, string email, string cpf, string senha, CancellationToken ct = default);

    /// <summary>Lanca AutenticacaoException se e-mail ou senha nao conferem.</summary>
    Task<Usuario> ValidarLoginAsync(string email, string senha, CancellationToken ct = default);
}
