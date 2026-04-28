using Maono.Domain.Common;
using Maono.Domain.Notifications.Entities;

namespace Maono.Domain.Notifications.Repository;

public interface INotificationRepository : IBaseRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(Guid userId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
