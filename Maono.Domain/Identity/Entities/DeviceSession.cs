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
/// Represents a user session bound to a specific device.
/// Each login creates a new session. Each session holds one active refresh token chain.
/// </summary>
public class DeviceSession : BaseEntity
{
    public Guid UserId { get; set; }
    public DeviceType DeviceType { get; set; } = DeviceType.Unknown;
    public string? DeviceName { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceFingerprint { get; set; }
    public DateTime LoginAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LogoutAtUtc { get; set; }
    public bool IsRevoked { get; set; }

    public bool IsActive => !IsRevoked && LogoutAtUtc == null;

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
