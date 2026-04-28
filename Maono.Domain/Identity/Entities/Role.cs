using Maono.Domain.Identity.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Identity.Entities;

/// <summary>
/// Application role. Can be global or workspace-scoped.
/// Examples: Admin, Strategist, Planner, Designer, ClientProxy, FreelancerOwner.
/// </summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }

    // Navigation
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    public ICollection<WorkspaceMembership> Memberships { get; set; } = new List<WorkspaceMembership>();
}
