using FluentValidation;
using Maono.Application.Features.Portal.Commands;

namespace Maono.Application.Features.Portal.Validators;

public class GeneratePortalTokenValidator : AbstractValidator<GeneratePortalTokenCommand>
{
    public GeneratePortalTokenValidator()
    {
        RuleFor(x => x.ClientOrganizationId).NotEmpty().WithMessage("L'identifiant du client est requis.");
        RuleFor(x => x.ExpiryHours)
            .InclusiveBetween(1, 720)
            .WithMessage("La durée de validité doit être entre 1 heure et 30 jours.");
    }
}

public class SubmitPortalDecisionValidator : AbstractValidator<SubmitPortalDecisionCommand>
{
    private static readonly string[] ValidDecisions = ["approved", "changes_requested"];

    public SubmitPortalDecisionValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("Le token est requis.");
        RuleFor(x => x.ContentItemId).NotEmpty().WithMessage("L'identifiant du contenu est requis.");
        RuleFor(x => x.Decision)
            .NotEmpty()
            .Must(d => ValidDecisions.Contains(d.ToLower()))
            .WithMessage("Décision invalide. Valeurs acceptées : 'approved', 'changes_requested'.");
        RuleFor(x => x.Comment)
            .MaximumLength(2000)
            .WithMessage("Le commentaire ne peut pas dépasser 2000 caractères.");
    }
}
