using Maono.Domain.Performance.Entities;
using Maono.Domain.Performance.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class PerformanceRepository : BaseRepository<PerformanceSnapshot>, IPerformanceRepository
{
    private readonly MaonoDbContext _context;

    public PerformanceRepository(MaonoDbContext context) : base(context) { _context = context; }

    public async Task<IReadOnlyList<PerformanceSnapshot>> GetByPublicationAsync(Guid publicationId, CancellationToken ct = default)
        => await DbSet.Where(s => s.PublicationId == publicationId).OrderByDescending(s => s.CollectedAtUtc).ToListAsync(ct);

    public async Task<IReadOnlyList<CampaignPerformanceAggregate>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default)
        => await _context.CampaignPerformanceAggregates.Where(a => a.CampaignId == campaignId).OrderByDescending(a => a.ComputedAtUtc).ToListAsync(ct);
}
