using Maono.Domain.Common;
using Maono.Domain.Planning.Entities;

namespace Maono.Domain.Planning.Repository;

public interface ICalendarRepository : IBaseRepository<CalendarEntry>
{
    Task<IReadOnlyList<CalendarEntry>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<IReadOnlyList<CalendarEntry>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
