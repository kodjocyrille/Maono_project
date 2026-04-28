using Maono.Domain.Notifications.Entities;
using Maono.Domain.Notifications.Repository;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence.Repositories;

public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(MaonoDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.Where(n => n.UserId == userId && n.ReadAtUtc == null).OrderByDescending(n => n.CreatedAtUtc).ToListAsync(ct);

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await DbSet.Where(n => n.UserId == userId && n.ReadAtUtc == null).ToListAsync(ct);
        foreach (var n in unread) n.ReadAtUtc = DateTime.UtcNow;
    }
}
