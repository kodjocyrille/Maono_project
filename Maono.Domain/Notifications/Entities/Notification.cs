using Maono.Domain.Notifications.Entities;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Publications.Entities;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Assets.Entities;
using Maono.Domain.Content.Entities;
using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Common;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Content.Enums;
using Maono.Domain.Assets.Enums;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Notifications.Enums;

namespace Maono.Domain.Notifications.Entities;

public class Notification : TenantEntity
{
    public Guid UserId { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;
    public string Type { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? Status { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public string? ReferenceEntityType { get; set; }
    public Guid? ReferenceEntityId { get; set; }
}
