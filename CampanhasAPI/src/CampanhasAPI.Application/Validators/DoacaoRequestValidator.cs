using CampanhasAPI.Application.Dtos.Requests;
using FluentValidation;

namespace CampanhasAPI.Application.Validators;

public class DoacaoRequestValidator : AbstractValidator<DoacaoRequest>
{
    public DoacaoRequestValidator()
    {
        RuleFor(x => x.IdCampanha).NotEmpty();
        RuleFor(x => x.ValorDoacao).GreaterThan(0).WithMessage("O valor da doação deve ser maior que zero.");
    }
}
