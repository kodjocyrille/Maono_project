using Maono.Domain.Planning.Entities;
using Maono.Domain.Planning.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class CalendarRepository : BaseRepository<CalendarEntry>, ICalendarRepository
{
    public CalendarRepository(MaonoDbContext context) : base(context) { }

    public async Task<IReadOnlyList<CalendarEntry>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await DbSet.Where(e => e.PublicationDate >= from && e.PublicationDate <= to).OrderBy(e => e.PublicationDate).ToListAsync(ct);

    public async Task<IReadOnlyList<CalendarEntry>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default)
        => await DbSet.Where(e => e.CampaignId == campaignId).OrderBy(e => e.PublicationDate).ToListAsync(ct);
}
