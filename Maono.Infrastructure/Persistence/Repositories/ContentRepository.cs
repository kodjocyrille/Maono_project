using Maono.Domain.Content.Entities;
using Maono.Domain.Content.Enums;
using Maono.Domain.Content.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class ContentRepository : BaseRepository<ContentItem>, IContentRepository
{
    public ContentRepository(MaonoDbContext context) : base(context) { }

    public async Task<ContentItem?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(c => c.Briefs)
            .Include(c => c.ChecklistItems)
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ContentItem>> GetByStatusAsync(ContentStatus status, CancellationToken ct = default)
        => await DbSet.Where(c => c.Status == status).ToListAsync(ct);

    public async Task<IReadOnlyList<ContentItem>> GetApproachingDeadlineAsync(DateTime deadline, CancellationToken ct = default)
        => await DbSet.Where(c => c.Deadline.HasValue && c.Deadline.Value <= deadline && c.Status != ContentStatus.Published)
            .OrderBy(c => c.Deadline)
            .ToListAsync(ct);
}
