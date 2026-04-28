using Maono.Domain.Common;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Missions.Enums;

namespace Maono.Domain.Missions.Repository;

public interface IMissionRepository : IBaseRepository<Mission>
{
    Task<Mission?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Mission>> GetByStatusAsync(MissionStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<Mission>> GetByOwnerAsync(Guid userId, CancellationToken ct = default);
}
