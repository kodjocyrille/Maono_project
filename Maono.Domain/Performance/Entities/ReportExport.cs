using Maono.Domain.Performance.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Performance.Entities;

public class ReportExport : TenantEntity
{
    public Guid? CampaignId { get; set; }
    public Guid? ClientOrganizationId { get; set; }
    public string Format { get; set; } = "PDF";
    public string? StoragePath { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public string? GeneratedBy { get; set; }
}
