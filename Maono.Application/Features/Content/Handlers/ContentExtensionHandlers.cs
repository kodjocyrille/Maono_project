using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Content.Commands;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Content.Entities;
using Maono.Domain.Content.Repository;
using Maono.Domain.Common;
using MediatR;

namespace Maono.Application.Features.Content.Handlers;

/// <summary>
/// ECR-011 — Add a dependency between two content items.
/// Validates: no self-reference.
/// </summary>
public class AddContentDependencyHandler : IRequestHandler<AddContentDependencyCommand, Result>
{
    private readonly IContentRepository _contentRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;
    private readonly IGenericRepository<ContentDependency> _depRepo;

    public AddContentDependencyHandler(
        IContentRepository contentRepo,
        ICurrentUserService currentUser,
        IUnitOfWork uow,
        IGenericRepository<ContentDependency> depRepo)
    {
        _contentRepo = contentRepo;
        _currentUser = currentUser;
        _uow = uow;
        _depRepo = depRepo;
    }

    public async Task<Result> Handle(AddContentDependencyCommand request, CancellationToken ct)
    {
        if (request.SourceContentId == request.BlockingContentId)
            return Result.Failure("Un contenu ne peut pas dépendre de lui-même.", "SELF_REFERENCE");

        var source = await _contentRepo.GetByIdAsync(request.SourceContentId, ct);
        if (source == null) return Result.Failure("Contenu source introuvable.", "NOT_FOUND");

        var blocking = await _contentRepo.GetByIdAsync(request.BlockingContentId, ct);
        if (blocking == null) return Result.Failure("Contenu bloquant introuvable.", "NOT_FOUND");

        // Check for duplicate
        var exists = await _depRepo.AnyAsync(d =>
            d.SourceContentId == request.SourceContentId && d.BlockingContentId == request.BlockingContentId, ct);
        if (exists) return Result.Failure("Cette dépendance existe déjà.", "DUPLICATE");

        var dep = new ContentDependency
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            SourceContentId = request.SourceContentId,
            BlockingContentId = request.BlockingContentId,
            DependencyType = request.DependencyType
        };
        await _depRepo.AddAsync(dep, ct);

        return Result.Success();
    }
}

/// <summary>
/// ECR-011 — Remove a content dependency.
/// </summary>
public class RemoveContentDependencyHandler : IRequestHandler<RemoveContentDependencyCommand, Result>
{
    private readonly IGenericRepository<ContentDependency> _depRepo;
    public RemoveContentDependencyHandler(IGenericRepository<ContentDependency> depRepo) => _depRepo = depRepo;

    public async Task<Result> Handle(RemoveContentDependencyCommand request, CancellationToken ct)
    {
        var dep = await _depRepo.GetByIdAsync(request.DependencyId, ct);
        if (dep == null) return Result.Failure("Dépendance introuvable.", "NOT_FOUND");
        _depRepo.Remove(dep);
        return Result.Success();
    }
}

/// <summary>
/// ECR-017 — Create a visual annotation on an asset version.
/// </summary>
public class CreateAnnotationHandler : IRequestHandler<CreateAnnotationCommand, Result<Guid>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IGenericRepository<ContentAnnotation> _annotRepo;

    public CreateAnnotationHandler(ICurrentUserService currentUser, IGenericRepository<ContentAnnotation> annotRepo)
    {
        _currentUser = currentUser;
        _annotRepo = annotRepo;
    }

    public async Task<Result<Guid>> Handle(CreateAnnotationCommand request, CancellationToken ct)
    {
        var annotation = new ContentAnnotation
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            AssetVersionId = request.AssetVersionId,
            CoordinatesJson = request.CoordinatesJson,
            Body = request.Body,
            AuthorId = _currentUser.UserId,
            PostedAtUtc = DateTime.UtcNow
        };
        await _annotRepo.AddAsync(annotation, ct);
        return Result.Success(annotation.Id);
    }
}

/// <summary>
/// ECR-017 — Delete an annotation.
/// </summary>
public class DeleteAnnotationHandler : IRequestHandler<DeleteAnnotationCommand, Result>
{
    private readonly IGenericRepository<ContentAnnotation> _annotRepo;
    public DeleteAnnotationHandler(IGenericRepository<ContentAnnotation> annotRepo) => _annotRepo = annotRepo;

    public async Task<Result> Handle(DeleteAnnotationCommand request, CancellationToken ct)
    {
        var annotation = await _annotRepo.GetByIdAsync(request.AnnotationId, ct);
        if (annotation == null) return Result.Failure("Annotation introuvable.", "NOT_FOUND");
        _annotRepo.Remove(annotation);
        return Result.Success();
    }
}
