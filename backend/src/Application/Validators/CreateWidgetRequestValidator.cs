using EnviroWatch.Application.DTOs;
using FluentValidation;

namespace EnviroWatch.Application.Validators;

public class CreateWidgetRequestValidator : AbstractValidator<CreateWidgetRequest>
{
    public CreateWidgetRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Metric).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Region).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Source).MaximumLength(64).When(x => x.Source is not null);
    }
}
