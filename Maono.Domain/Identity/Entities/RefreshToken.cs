using Maono.Domain.Identity.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Identity.Entities;

/// <summary>
/// Long-lived refresh token for JWT session management.
/// Always linked to a DeviceSession for device tracking.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid DeviceSessionId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? RevokedReason { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation
    public User User { get; set; } = null!;
    public DeviceSession DeviceSession { get; set; } = null!;
}
