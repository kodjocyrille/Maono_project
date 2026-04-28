using Maono.Domain.Clients.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Clients.Entities;

/// <summary>
/// Brand identity profile for a client.
/// </summary>
public class BrandProfile : TenantEntity
{
    public Guid ClientOrganizationId { get; set; }
    public string? BrandTone { get; set; }
    public string? Palette { get; set; }
    public string? Constraints { get; set; }
    public string? SocialHandles { get; set; }
    public string? LogoUrl { get; set; }
    public string? GuidelinesUrl { get; set; }

    // Navigation
    public ClientOrganization ClientOrganization { get; set; } = null!;
}
