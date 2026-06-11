using EnviroWatch.Application.DTOs;
using FluentValidation;

namespace EnviroWatch.Application.Validators;

public class CreateDashboardRequestValidator : AbstractValidator<CreateDashboardRequest>
{
    public CreateDashboardRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(512).When(x => x.Description is not null);
    }
}
