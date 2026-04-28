using Maono.Domain.Content.Entities;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Identity.Entities;

/// <summary>
/// Fine-grained permission. Associated to roles.
/// Examples: campaign.create, publication.schedule, content.approve.internal.
/// </summary>
public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation
    public ICollection<Role> Roles { get; set; } = new List<Role>();
}
