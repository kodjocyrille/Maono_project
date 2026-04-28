using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Content.Enums;
using Maono.Domain.Content.Repository;
using MediatR;

namespace Maono.Application.Features.Content;

// ── ECR-033 — Full-text search on ContentItems ──────────

public record SearchContentsQuery(
    string? Keyword,
    string? Status,
    string? Format,
    int? MinPriority
) : IQuery<Result<List<ContentSearchResultDto>>>;

public record ContentSearchResultDto(
    Guid Id,
    string Title,
    string? Format,
    string Status,
    int Priority,
    DateTime? Deadline,
    DateTime CreatedAtUtc
);

public class SearchContentsHandler : IRequestHandler<SearchContentsQuery, Result<List<ContentSearchResultDto>>>
{
    private readonly IContentRepository _contentRepo;
    public SearchContentsHandler(IContentRepository contentRepo) => _contentRepo = contentRepo;

    public async Task<Result<List<ContentSearchResultDto>>> Handle(SearchContentsQuery request, CancellationToken ct)
    {
        // Start with all content items
        var items = await _contentRepo.GetAllAsync(ct);
        var results = items.AsEnumerable();

        // ECR-033 — Apply filters
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.ToLowerInvariant();
            results = results.Where(c => c.Title.ToLowerInvariant().Contains(keyword)
                || (c.Format?.ToLowerInvariant().Contains(keyword) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ContentStatus>(request.Status, true, out var status))
            results = results.Where(c => c.Status == status);

        if (!string.IsNullOrWhiteSpace(request.Format))
            results = results.Where(c => c.Format != null && c.Format.Equals(request.Format, StringComparison.OrdinalIgnoreCase));

        if (request.MinPriority.HasValue)
            results = results.Where(c => c.Priority >= request.MinPriority.Value);

        var dtos = results.Select(c => new ContentSearchResultDto(
            c.Id, c.Title, c.Format, c.Status.ToString(), c.Priority, c.Deadline, c.CreatedAtUtc
        )).OrderByDescending(c => c.CreatedAtUtc).ToList();

        return Result.Success(dtos);
    }
}

// ── ECR-035 — Auto-archive old published content ─────────

public record ArchiveOldContentsCommand(int DaysAfterPublish = 90) : ICommand<Result<int>>;

public class ArchiveOldContentsHandler : IRequestHandler<ArchiveOldContentsCommand, Result<int>>
{
    private readonly IContentRepository _contentRepo;
    public ArchiveOldContentsHandler(IContentRepository contentRepo) => _contentRepo = contentRepo;

    public async Task<Result<int>> Handle(ArchiveOldContentsCommand request, CancellationToken ct)
    {
        var threshold = DateTime.UtcNow.AddDays(-request.DaysAfterPublish);
        var published = await _contentRepo.GetByStatusAsync(ContentStatus.Published, ct);
        var toArchive = published.Where(c => c.UpdatedAtUtc < threshold).ToList();

        foreach (var item in toArchive)
        {
            item.Status = ContentStatus.Archived;
            _contentRepo.Update(item);
        }

        return Result.Success(toArchive.Count);
    }
}

// ── ECR-036/037 — RGPD: Data export + Purge ─────────────

public record ExportUserDataQuery(Guid UserId) : IQuery<Result<UserDataExportDto>>;
public record PurgeUserDataCommand(Guid UserId) : ICommand<Result<int>>;

public record UserDataExportDto(
    Guid UserId,
    int ContentItemsCount,
    int TasksCount,
    int MessagesCount,
    int NotificationsCount,
    DateTime ExportedAtUtc
);

public class ExportUserDataHandler : IRequestHandler<ExportUserDataQuery, Result<UserDataExportDto>>
{
    private readonly Domain.Common.IGenericRepository<Domain.Content.Entities.ContentTask> _taskRepo;
    private readonly Domain.Common.IGenericRepository<Domain.Approval.Entities.ContentMessage> _messageRepo;
    private readonly Domain.Common.IGenericRepository<Domain.Notifications.Entities.Notification> _notifRepo;

    public ExportUserDataHandler(
        Domain.Common.IGenericRepository<Domain.Content.Entities.ContentTask> taskRepo,
        Domain.Common.IGenericRepository<Domain.Approval.Entities.ContentMessage> messageRepo,
        Domain.Common.IGenericRepository<Domain.Notifications.Entities.Notification> notifRepo)
    {
        _taskRepo = taskRepo;
        _messageRepo = messageRepo;
        _notifRepo = notifRepo;
    }

    public async Task<Result<UserDataExportDto>> Handle(ExportUserDataQuery request, CancellationToken ct)
    {
        var tasks = await _taskRepo.FindAsync(t => t.AssignedToUserId == request.UserId, ct);
        var messages = await _messageRepo.FindAsync(m => m.AuthorId == request.UserId, ct);
        var notifs = await _notifRepo.FindAsync(n => n.UserId == request.UserId, ct);

        return Result.Success(new UserDataExportDto(
            request.UserId,
            0, // ContentItems are not user-owned directly
            tasks.Count,
            messages.Count,
            notifs.Count,
            DateTime.UtcNow));
    }
}

public class PurgeUserDataHandler : IRequestHandler<PurgeUserDataCommand, Result<int>>
{
    private readonly Domain.Common.IGenericRepository<Domain.Notifications.Entities.Notification> _notifRepo;

    public PurgeUserDataHandler(Domain.Common.IGenericRepository<Domain.Notifications.Entities.Notification> notifRepo)
        => _notifRepo = notifRepo;

    public async Task<Result<int>> Handle(PurgeUserDataCommand request, CancellationToken ct)
    {
        var notifs = await _notifRepo.FindAsync(n => n.UserId == request.UserId, ct);
        _notifRepo.RemoveRange(notifs);
        return Result.Success(notifs.Count);
    }
}
