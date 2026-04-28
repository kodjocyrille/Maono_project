using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class SessionRepository : BaseRepository<DeviceSession>, ISessionRepository
{
    public SessionRepository(MaonoDbContext context) : base(context) { }

    public async Task<IReadOnlyList<DeviceSession>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.Where(s => s.UserId == userId && !s.IsRevoked && s.LogoutAtUtc == null)
            .OrderByDescending(s => s.LastActiveAtUtc)
            .ToListAsync(ct);

    public async Task RevokeAllSessionsExceptAsync(Guid userId, Guid keepSessionId, CancellationToken ct = default)
    {
        var sessions = await DbSet
            .Where(s => s.UserId == userId && s.Id != keepSessionId && !s.IsRevoked)
            .ToListAsync(ct);
        foreach (var session in sessions)
        {
            session.IsRevoked = true;
            session.LogoutAtUtc = DateTime.UtcNow;
        }
    }
}
