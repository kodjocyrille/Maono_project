using Maono.Domain.Approval.Entities;
using Maono.Domain.Approval.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class ApprovalRepository : BaseRepository<ApprovalCycle>, IApprovalRepository
{
    public ApprovalRepository(MaonoDbContext context) : base(context) { }

    public async Task<ApprovalCycle?> GetWithDecisionsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.Include(c => c.Decisions).FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ApprovalCycle>> GetByContentAsync(Guid contentItemId, CancellationToken ct = default)
        => await DbSet.Where(c => c.ContentItemId == contentItemId).OrderBy(c => c.RevisionRound).ToListAsync(ct);

    public async Task<IReadOnlyList<ApprovalCycle>> GetPendingAsync(CancellationToken ct = default)
        => await DbSet.Where(c => c.CompletedAtUtc == null).ToListAsync(ct);
}
