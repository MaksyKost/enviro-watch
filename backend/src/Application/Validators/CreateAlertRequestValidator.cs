using EnviroWatch.Application.DTOs;
using FluentValidation;

namespace EnviroWatch.Application.Validators;

public class CreateAlertRequestValidator : AbstractValidator<CreateAlertRequest>
{
    public CreateAlertRequestValidator()
    {
        RuleFor(x => x.Metric).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Region).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Condition).IsInEnum();
    }
}
