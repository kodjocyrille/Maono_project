using Maono.Domain.Missions.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Missions.Entities;

public class MissionTask : TenantEntity
{
    public Guid MissionId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public string? Status { get; set; }

    // Navigation
    public Mission Mission { get; set; } = null!;
}
