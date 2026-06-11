using EnviroWatch.Application.DTOs;
using FluentValidation;

namespace EnviroWatch.Application.Validators;

public class CreateObservationRequestValidator : AbstractValidator<CreateObservationRequest>
{
    public CreateObservationRequestValidator()
    {
        RuleFor(x => x.Region).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Metric).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Unit).MaximumLength(16).When(x => x.Unit is not null);
        RuleFor(x => x.Notes).MaximumLength(512).When(x => x.Notes is not null);
    }
}
