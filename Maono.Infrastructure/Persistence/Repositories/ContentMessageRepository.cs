using Maono.Domain.Approval.Entities;
using Maono.Domain.Approval.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class ContentMessageRepository : BaseRepository<ContentMessage>, IContentMessageRepository
{
    public ContentMessageRepository(MaonoDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ContentMessage>> GetByContentItemAsync(Guid contentItemId, CancellationToken ct = default)
        => await DbSet.Where(m => m.ContentItemId == contentItemId).OrderBy(m => m.SentAtUtc).ToListAsync(ct);
}
