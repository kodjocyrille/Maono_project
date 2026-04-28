using FluentValidation;
using Maono.Application.Features.Content.Commands;

namespace Maono.Application.Features.Content.Validators;

public class CreateContentValidator : AbstractValidator<CreateContentCommand>
{
    public CreateContentValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Deadline).GreaterThan(DateTime.UtcNow)
            .When(x => x.Deadline.HasValue)
            .WithMessage("Deadline must be in the future");
    }
}

public class UpdateContentStatusValidator : AbstractValidator<UpdateContentStatusCommand>
{
    public UpdateContentStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}
