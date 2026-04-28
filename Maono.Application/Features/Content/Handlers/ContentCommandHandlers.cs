using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Content.Commands;
using Maono.Application.Features.Content.DTOs;
using Maono.Domain.Content.Entities;
using Maono.Domain.Content.Enums;
using Maono.Domain.Content.Repository;
using MediatR;

namespace Maono.Application.Features.Content.Handlers;

public class CreateContentHandler : IRequestHandler<CreateContentCommand, Result<ContentItemDto>>
{
    private readonly IContentRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public CreateContentHandler(IContentRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<ContentItemDto>> Handle(CreateContentCommand request, CancellationToken ct)
    {
        var item = new ContentItem
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            Title = request.Title,
            Format = request.Format,
            Deadline = request.Deadline,
            Priority = request.Priority,
            CalendarEntryId = request.CalendarEntryId,
            Status = ContentStatus.Draft
        };
        await _repo.AddAsync(item, ct);

        return Result.Success(new ContentItemDto(
            item.Id, item.Title, item.Format, item.Status,
            item.Deadline, item.Priority, item.CurrentRevisionNumber, item.CreatedAtUtc));
    }
}

/// <summary>
/// ECR-001 — State machine for content status transitions.
/// Only transitions defined in AllowedTransitions are permitted.
/// ECR-011 — Checks blocking dependencies before advancing.
/// </summary>
public class UpdateContentStatusHandler : IRequestHandler<UpdateContentStatusCommand, Result<ContentItemDto>>
{
    private readonly IContentRepository _repo;
    private readonly Domain.Common.IGenericRepository<ContentDependency> _depRepo;

    /// <summary>
    /// Allowed transitions per CdCF §7.1 and §4.1.
    /// Draft → InProduction → InReview → ClientReview → Approved → Scheduled → Published → Archived
    /// RevisionRequired → InProduction (re-enter production after client rejection)
    /// PublishFailed → Scheduled (retry)
    /// </summary>
    private static readonly Dictionary<ContentStatus, ContentStatus[]> AllowedTransitions = new()
    {
        [ContentStatus.Draft]             = [ContentStatus.InProduction],
        [ContentStatus.InProduction]      = [ContentStatus.InReview],
        [ContentStatus.InReview]          = [ContentStatus.ClientReview, ContentStatus.RevisionRequired],
        [ContentStatus.ClientReview]      = [ContentStatus.Approved, ContentStatus.RevisionRequired],
        [ContentStatus.RevisionRequired]  = [ContentStatus.InProduction],
        [ContentStatus.Approved]          = [ContentStatus.Scheduled],
        [ContentStatus.Scheduled]         = [ContentStatus.Published, ContentStatus.PublishFailed],
        [ContentStatus.Published]         = [ContentStatus.Archived],
        [ContentStatus.PublishFailed]     = [ContentStatus.Scheduled],
        [ContentStatus.Archived]          = [],
    };

    public UpdateContentStatusHandler(IContentRepository repo, Domain.Common.IGenericRepository<ContentDependency> depRepo)
    {
        _repo = repo;
        _depRepo = depRepo;
    }

    public async Task<Result<ContentItemDto>> Handle(UpdateContentStatusCommand request, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(request.Id, ct);
        if (item == null) return Result.Failure<ContentItemDto>("Contenu introuvable.", "NOT_FOUND");

        // ECR-001: Validate transition
        if (!AllowedTransitions.TryGetValue(item.Status, out var allowed) || !allowed.Contains(request.NewStatus))
        {
            return Result.Failure<ContentItemDto>(
                $"Transition non autorisée : {item.Status} → {request.NewStatus}. Transitions possibles : {string.Join(", ", allowed ?? [])}.",
                "INVALID_TRANSITION");
        }

        // ECR-011: Check blocking dependencies before advancing to InProduction
        if (request.NewStatus == ContentStatus.InProduction)
        {
            var blockingDeps = await _depRepo.FindAsync(d => d.SourceContentId == request.Id, ct);
            if (blockingDeps.Count > 0)
            {
                // Verify all blocking contents are completed (Published or Archived)
                foreach (var dep in blockingDeps)
                {
                    var blockingContent = await _repo.GetByIdAsync(dep.BlockingContentId, ct);
                    if (blockingContent != null &&
                        blockingContent.Status != ContentStatus.Published &&
                        blockingContent.Status != ContentStatus.Archived)
                    {
                        return Result.Failure<ContentItemDto>(
                            $"Contenu bloqué par la dépendance \"{blockingContent.Title}\" (statut : {blockingContent.Status}). " +
                            $"Le contenu bloquant doit être Published ou Archived avant de pouvoir avancer.",
                            "BLOCKED_BY_DEPENDENCY");
                    }
                }
            }
        }

        item.Status = request.NewStatus;
        _repo.Update(item);

        return Result.Success(new ContentItemDto(
            item.Id, item.Title, item.Format, item.Status,
            item.Deadline, item.Priority, item.CurrentRevisionNumber, item.CreatedAtUtc));
    }
}

public class UpdateContentHandler : IRequestHandler<UpdateContentCommand, Result>
{
    private readonly IContentRepository _repo;
    public UpdateContentHandler(IContentRepository repo) => _repo = repo;

    public async Task<Result> Handle(UpdateContentCommand request, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(request.Id, ct);
        if (item == null) return Result.Failure("Contenu introuvable.", "NOT_FOUND");
        item.Title = request.Title;
        if (request.Format != null) item.Format = request.Format;
        item.Priority = request.Priority;
        item.Deadline = request.Deadline;
        _repo.Update(item);
        return Result.Success();
    }
}

/// <summary>
/// ECR-005 — A Published content cannot be deleted, only archived.
/// </summary>
public class DeleteContentHandler : IRequestHandler<DeleteContentCommand, Result>
{
    private readonly IContentRepository _repo;
    public DeleteContentHandler(IContentRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeleteContentCommand request, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(request.Id, ct);
        if (item == null) return Result.Failure("Contenu introuvable.", "NOT_FOUND");

        // ECR-005: Prevent deletion of Published content
        if (item.Status == ContentStatus.Published)
            return Result.Failure("Un contenu publié ne peut pas être supprimé. Utilisez l'archivage.", "CANNOT_DELETE_PUBLISHED");

        item.IsDeleted = true;
        item.DeletedAtUtc = DateTime.UtcNow;
        _repo.Update(item);
        return Result.Success();
    }
}

public class GetContentByDeadlineHandler : IRequestHandler<Queries.GetContentByDeadlineQuery, Result<List<ContentItemDto>>>
{
    private readonly IContentRepository _repo;
    public GetContentByDeadlineHandler(IContentRepository repo) => _repo = repo;

    public async Task<Result<List<ContentItemDto>>> Handle(Queries.GetContentByDeadlineQuery request, CancellationToken ct)
    {
        var items = await _repo.GetApproachingDeadlineAsync(request.Deadline, ct);
        var dtos = items.Select(i => new ContentItemDto(
            i.Id, i.Title, i.Format, i.Status, i.Deadline, i.Priority, i.CurrentRevisionNumber, i.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}
