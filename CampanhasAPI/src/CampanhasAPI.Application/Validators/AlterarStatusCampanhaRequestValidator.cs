using CampanhasAPI.Application.Dtos.Requests;
using CampanhasAPI.Domain.Enums;
using FluentValidation;

namespace CampanhasAPI.Application.Validators;

public class AlterarStatusCampanhaRequestValidator : AbstractValidator<AlterarStatusCampanhaRequest>
{
    public AlterarStatusCampanhaRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum()
            .Must(s => s is StatusCampanha.Concluida or StatusCampanha.Cancelada)
            .WithMessage("Status deve ser Concluida ou Cancelada.");
    }
}
