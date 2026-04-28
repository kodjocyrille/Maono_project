using Maono.Domain.Common;
using Maono.Domain.Identity.Entities;

namespace Maono.Domain.Identity.Repository;

public interface ISessionRepository : IBaseRepository<DeviceSession>
{
    Task<IReadOnlyList<DeviceSession>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default);
    Task RevokeAllSessionsExceptAsync(Guid userId, Guid keepSessionId, CancellationToken ct = default);
}
