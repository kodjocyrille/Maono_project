using Maono.Domain.Common;
using Maono.Domain.Content.Entities;
using Maono.Domain.Content.Repository;
using Maono.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class ContentTaskRepository : BaseRepository<ContentTask>, IContentTaskRepository
{
    public ContentTaskRepository(MaonoDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ContentTask>> GetByContentItemAsync(Guid contentItemId, CancellationToken ct = default)
        => await Context.Set<ContentTask>()
            .Where(t => t.ContentItemId == contentItemId)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ContentTask>> GetByAssignedUserAsync(Guid userId, ContentTaskStatus? status, CancellationToken ct = default)
        => await Context.Set<ContentTask>()
            .Where(t => t.AssignedToUserId == userId)
            .Where(t => status == null || t.Status == status)
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .ThenByDescending(t => t.Priority)
            .ToListAsync(ct);
}
