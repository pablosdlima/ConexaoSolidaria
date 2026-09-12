using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Domain.Validators;
using FluentValidation;

namespace CampanhasAPI.Application.Validators;

public class CadastrarDoadorRequestValidator : AbstractValidator<CadastrarDoadorRequest>
{
    public CadastrarDoadorRequestValidator()
    {
        RuleFor(x => x.NomeCompleto).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Cpf).NotEmpty().Must(CpfValidator.EhValido).WithMessage("CPF inválido.");
        RuleFor(x => x.Senha).NotEmpty().MinimumLength(8).WithMessage("A senha deve ter ao menos 8 caracteres.");
    }
}
