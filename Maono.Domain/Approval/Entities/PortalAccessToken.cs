using Maono.Domain.Common;

namespace Maono.Domain.Approval.Entities;

/// <summary>
/// Ephemeral access token for client portal.
/// Allows external clients to view and approve/request changes on content
/// without a user account.
/// </summary>
public class PortalAccessToken : TenantEntity
{
    public Guid ClientOrganizationId { get; set; }
    public Guid? ContentItemId { get; set; }          // null = all contents for client
    public Guid? CampaignId { get; set; }             // null = not scoped to campaign

    public string Token { get; set; } = string.Empty;  // cryptographically random, URL-safe
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }
    public Guid CreatedByUserId { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;
}
