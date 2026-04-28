using Maono.Domain.Missions.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Missions.Entities;

public class DeliveryNote : TenantEntity
{
    public Guid MissionDeliveryId { get; set; }
    public string? Reference { get; set; }
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public string? StoragePath { get; set; }

    // Navigation
    public MissionDelivery MissionDelivery { get; set; } = null!;
}
