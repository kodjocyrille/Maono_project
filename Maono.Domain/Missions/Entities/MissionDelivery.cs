using Maono.Domain.Missions.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Missions.Entities;

public class MissionDelivery : TenantEntity
{
    public Guid MissionMilestoneId { get; set; }
    public DateTime DeliveredAtUtc { get; set; } = DateTime.UtcNow;
    public string? DeliveredBy { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public MissionMilestone Milestone { get; set; } = null!;
    public DeliveryNote? DeliveryNote { get; set; }
}
