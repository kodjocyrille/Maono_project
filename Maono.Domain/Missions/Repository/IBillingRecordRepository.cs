using Maono.Domain.Common;
using Maono.Domain.Missions.Entities;

namespace Maono.Domain.Missions.Repository;

public interface IBillingRecordRepository : IBaseRepository<BillingRecord>
{
    Task<IReadOnlyList<BillingRecord>> GetPendingExportAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BillingRecord>> GetByMissionAsync(Guid missionId, CancellationToken ct = default);
}
