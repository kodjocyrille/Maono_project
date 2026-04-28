using Maono.Domain.Publications.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Publications.Entities;

public class PublicationLog : TenantEntity
{
    public Guid PublicationId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Publication Publication { get; set; } = null!;
}
