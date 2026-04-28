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

namespace Maono.Domain.Identity.Entities;

/// <summary>
/// Workspace represents a logical tenant.
/// Aggregate root for tenant governance.
/// </summary>
public class Workspace : AggregateRoot, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Plan { get; set; }
    public WorkspaceStatus Status { get; set; } = WorkspaceStatus.Active;
    public string? DefaultTimezone { get; set; }
    public string? SettingsJson { get; set; }
    public string? LogoUrl { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public ICollection<WorkspaceMembership> Memberships { get; set; } = new List<WorkspaceMembership>();
}
