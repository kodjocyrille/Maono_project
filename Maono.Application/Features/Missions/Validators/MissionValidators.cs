using FluentValidation;
using Maono.Application.Features.Missions.Commands;

namespace Maono.Application.Features.Missions.Validators;

public class CreateMissionValidator : AbstractValidator<CreateMissionCommand>
{
    public CreateMissionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
        RuleFor(x => x.Budget).GreaterThan(0)
            .When(x => x.Budget.HasValue);
    }
}

public class UpdateMissionStatusValidator : AbstractValidator<UpdateMissionStatusCommand>
{
    public UpdateMissionStatusValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}
