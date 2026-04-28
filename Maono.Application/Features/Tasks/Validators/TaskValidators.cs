using FluentValidation;
using Maono.Application.Features.Tasks.Commands;
using Maono.Domain.Content.Entities;

namespace Maono.Application.Features.Tasks.Validators;

public class CreateTaskValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.ContentItemId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public class UpdateTaskValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        When(x => x.Title != null, () =>
            RuleFor(x => x.Title).MaximumLength(200));
        When(x => x.Status == ContentTaskStatus.Blocked, () =>
            RuleFor(x => x.BlockedReason)
                .NotEmpty().WithMessage("La raison du blocage est requise.")
                .MaximumLength(500));
    }
}
