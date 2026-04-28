using Maono.Domain.Clients.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Clients.Entities;

/// <summary>
/// Contact person within a client organization.
/// </summary>
public class ClientContact : TenantEntity
{
    public Guid ClientOrganizationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public bool IsPrimaryApprover { get; set; }

    // Navigation
    public ClientOrganization ClientOrganization { get; set; } = null!;
}
