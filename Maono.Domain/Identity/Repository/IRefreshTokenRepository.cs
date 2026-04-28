using Maono.Domain.Common;
using Maono.Domain.Identity.Entities;

namespace Maono.Domain.Identity.Repository;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken ct = default);
    Task RevokeAllForSessionAsync(Guid sessionId, string reason, CancellationToken ct = default);
}
