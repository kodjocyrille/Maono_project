using Maono.Domain.Common;
using Maono.Domain.Performance.Entities;

namespace Maono.Domain.Performance.Repository;

public interface IPerformanceRepository : IBaseRepository<PerformanceSnapshot>
{
    Task<IReadOnlyList<PerformanceSnapshot>> GetByPublicationAsync(Guid publicationId, CancellationToken ct = default);
    Task<IReadOnlyList<CampaignPerformanceAggregate>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
