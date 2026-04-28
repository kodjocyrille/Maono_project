using Maono.Domain.Planning.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Planning.Entities;

public class ResourceCapacity : TenantEntity
{
    public Guid UserId { get; set; }
    public DateTime WeekStart { get; set; }
    public decimal CapacityHours { get; set; }
    public decimal AssignedHours { get; set; }
    public decimal? OverloadThreshold { get; set; }
}
