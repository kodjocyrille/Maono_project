using FluentValidation;
using Maono.Application.Features.Assets.Commands;

namespace Maono.Application.Features.Assets.Validators;

public class InitiateUploadSessionValidator : AbstractValidator<InitiateUploadSessionCommand>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml",
        "video/mp4", "video/quicktime", "video/x-msvideo", "video/webm",
        "audio/mpeg", "audio/wav", "audio/ogg",
        "application/pdf", "application/zip",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation"
    ];

    // 2 GB default max
    private const long MaxFileSizeBytes = 2L * 1024 * 1024 * 1024;

    public InitiateUploadSessionValidator()
    {
        RuleFor(x => x.ContentItemId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.MimeType)
            .NotEmpty()
            .Must(m => AllowedMimeTypes.Any(a => m.StartsWith(a.Split('/')[0])))
            .WithMessage("Type MIME non autorisé.");
        RuleFor(x => x.DeclaredSizeBytes)
            .GreaterThan(0).WithMessage("La taille doit être supérieure à 0.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("La taille maximale est de 2 Go.");
        RuleFor(x => x.DeclaredSha256)
            .NotEmpty()
            .Length(64).WithMessage("Le SHA-256 doit faire 64 caractères hexadécimaux.")
            .Matches("^[a-fA-F0-9]+$").WithMessage("Le SHA-256 doit être en hexadécimal.");
    }
}

public class ConfirmUploadSessionValidator : AbstractValidator<ConfirmUploadSessionCommand>
{
    public ConfirmUploadSessionValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.ActualSizeBytes).GreaterThan(0);
        RuleFor(x => x.ActualSha256)
            .NotEmpty()
            .Length(64)
            .Matches("^[a-fA-F0-9]+$");
    }
}
