using FluentValidation;
using Maono.Application.Features.Auth.Commands;

namespace Maono.Application.Features.Auth.Validators;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50)
            .WithMessage("Le prénom est requis.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50)
            .WithMessage("Le nom est requis.");
        RuleFor(x => x.WorkspaceName).NotEmpty().MaximumLength(100)
            .WithMessage("Le nom du workspace est requis.");
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-]{6,20}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Format de numéro de téléphone invalide.");
    }
}

public class RegisterByInviteValidator : AbstractValidator<RegisterByInviteCommand>
{
    public RegisterByInviteValidator()
    {
        RuleFor(x => x.InviteToken).NotEmpty()
            .WithMessage("Le token d'invitation est requis.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50)
            .WithMessage("Le prénom est requis.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50)
            .WithMessage("Le nom est requis.");
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-]{6,20}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Format de numéro de téléphone invalide.");
    }
}

public class AdminCreateUserValidator : AbstractValidator<AdminCreateUserCommand>
{
    public AdminCreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50)
            .WithMessage("Le prénom est requis.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50)
            .WithMessage("Le nom est requis.");
        RuleFor(x => x.WorkspaceId).NotEmpty()
            .WithMessage("L'identifiant du workspace est requis.");
        RuleFor(x => x.RoleName).NotEmpty()
            .WithMessage("Le nom du rôle est requis.");
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[0-9\s\-]{6,20}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Format de numéro de téléphone invalide.");
    }
}
