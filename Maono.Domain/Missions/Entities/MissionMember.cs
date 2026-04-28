using Maono.Domain.Missions.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Missions.Entities;

public class MissionMember : TenantEntity
{
    public Guid MissionId { get; set; }
    public Guid UserId { get; set; }
    public string? RoleOnMission { get; set; }
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Mission Mission { get; set; } = null!;
}
