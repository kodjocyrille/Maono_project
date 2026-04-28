using Maono.Domain.Clients.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Clients.Entities;

/// <summary>
/// Client organization within a workspace.
/// </summary>
public class ClientOrganization : TenantEntity, ISoftDeletable
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? BillingEmail { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public string? ExternalOdooId { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public ICollection<ClientContact> Contacts { get; set; } = new List<ClientContact>();
    public BrandProfile? BrandProfile { get; set; }
}
