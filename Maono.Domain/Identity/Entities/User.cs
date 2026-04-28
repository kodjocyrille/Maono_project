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
/// Application user. Global entity (not workspace-scoped).
/// Links to ASP.NET Core Identity via IdentityId.
/// </summary>
public class User : BaseEntity, ISoftDeletable
{
    /// <summary>
    /// The ASP.NET Core Identity user ID (from IdentityUser).
    /// </summary>
    public string IdentityId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    /// <summary>
    /// ECR-022 — Freelance profile type (CdCF §3.2). None for agency users.
    /// </summary>
    public FreelanceProfile FreelanceProfile { get; set; } = FreelanceProfile.None;
    public DateTime? LastLoginAtUtc { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public ICollection<WorkspaceMembership> Memberships { get; set; } = new List<WorkspaceMembership>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<DeviceSession> DeviceSessions { get; set; } = new List<DeviceSession>();
}
