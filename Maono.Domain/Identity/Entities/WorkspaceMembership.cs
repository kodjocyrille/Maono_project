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
/// Links a User to a Workspace with a specific role.
/// </summary>
public class WorkspaceMembership : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid RoleId { get; set; }
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsDefault { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
