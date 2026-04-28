using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(MaonoDbContext context) : base(context) { }

    public async Task<RefreshToken?> GetActiveByTokenAsync(string token, CancellationToken ct = default)
    {
        var refreshToken = await DbSet.FirstOrDefaultAsync(t => t.Token == token && t.RevokedAtUtc == null, ct);
        return refreshToken?.IsExpired == true ? null : refreshToken;
    }

    public async Task RevokeAllForSessionAsync(Guid sessionId, string reason, CancellationToken ct = default)
    {
        var tokens = await DbSet
            .Where(t => t.DeviceSessionId == sessionId && t.RevokedAtUtc == null)
            .ToListAsync(ct);
        foreach (var token in tokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedReason = reason;
        }
    }
}
