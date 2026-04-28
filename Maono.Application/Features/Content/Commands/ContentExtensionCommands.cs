using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Content.Commands;

// ── ECR-011 — Content Dependencies ──────────────────────────────

public record AddContentDependencyCommand(
    Guid SourceContentId,
    Guid BlockingContentId,
    string? DependencyType
) : ICommand<Result>;

public record RemoveContentDependencyCommand(Guid DependencyId) : ICommand<Result>;

// ── ECR-017 — Content Annotations ───────────────────────────────

public record CreateAnnotationCommand(
    Guid AssetVersionId,
    string? CoordinatesJson,
    string Body
) : ICommand<Result<Guid>>;

public record DeleteAnnotationCommand(Guid AnnotationId) : ICommand<Result>;
