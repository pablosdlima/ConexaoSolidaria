using System.Net.Mail;
using CampanhasAPI.Domain.Enums;
using CampanhasAPI.Domain.Exceptions;
using CampanhasAPI.Domain.Validators;

namespace CampanhasAPI.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string Cpf { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public Role Role { get; private set; }
    public DateTimeOffset DataCriacao { get; private set; }

    private Usuario() { } // EF Core

    /// <param name="senhaHash">Já com hash (BCrypt). A entidade nunca vê a senha em texto puro.</param>
    public static Usuario Criar(string nomeCompleto, string email, string cpf, string senhaHash, Role role)
    {
        DomainException.Quando(string.IsNullOrWhiteSpace(nomeCompleto), "O nome completo é obrigatório.");
        DomainException.Quando(!MailAddress.TryCreate(email, out _), "O e-mail é inválido.");
        DomainException.Quando(!CpfValidator.EhValido(cpf), "O CPF é inválido.");
        DomainException.Quando(string.IsNullOrWhiteSpace(senhaHash), "O hash da senha é obrigatório.");

        return new Usuario
        {
            Id = Guid.NewGuid(),
            NomeCompleto = nomeCompleto.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Cpf = CpfValidator.Normalizar(cpf),
            SenhaHash = senhaHash,
            Role = role,
            DataCriacao = DateTimeOffset.UtcNow
        };
    }
}
