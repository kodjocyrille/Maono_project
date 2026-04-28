using Maono.Domain.Missions.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Missions.Entities;

public class MissionMilestone : TenantEntity
{
    public Guid MissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
    public decimal? Amount { get; set; }

    // Navigation
    public Mission Mission { get; set; } = null!;
    public ICollection<MissionDelivery> Deliveries { get; set; } = new List<MissionDelivery>();
}
