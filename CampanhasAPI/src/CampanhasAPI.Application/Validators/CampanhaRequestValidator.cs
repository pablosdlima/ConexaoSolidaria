using CampanhasAPI.Application.Dtos.Requests;
using FluentValidation;

namespace CampanhasAPI.Application.Validators;

/// <summary>
/// Valida forma e limites. As regras de negócio (data no passado, meta > 0)
/// ficam na entidade Campanha — aqui só se garante que o payload faz sentido.
/// </summary>
public class CampanhaRequestValidator : AbstractValidator<CampanhaRequest>
{
    public CampanhaRequestValidator()
    {
        RuleFor(x => x.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Descricao).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.MetaFinanceira).GreaterThan(0).WithMessage("A meta financeira deve ser maior que zero.");
        RuleFor(x => x.DataFim).GreaterThanOrEqualTo(x => x.DataInicio)
            .WithMessage("A data de término não pode ser anterior à data de início.");
    }
}
