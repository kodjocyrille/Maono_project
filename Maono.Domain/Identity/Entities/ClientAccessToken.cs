using Maono.Domain.Identity.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Identity.Entities;

/// <summary>
/// Magic link / client portal access token.
/// Single-use, time-limited access for external client contacts.
/// </summary>
public class ClientAccessToken : TenantEntity
{
    public string Token { get; set; } = string.Empty;
    public Guid? ContentItemId { get; set; }
    public Guid? ApprovalCycleId { get; set; }
    public Guid? ClientContactId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsUsed => UsedAtUtc.HasValue;
    public bool IsValid => !IsExpired && !IsUsed;
}
