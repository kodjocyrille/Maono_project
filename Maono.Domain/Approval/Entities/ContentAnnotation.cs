using Maono.Domain.Approval.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Approval.Entities;

/// <summary>
/// Structured comment on a preview asset. Prepared for v2.
/// </summary>
public class ContentAnnotation : TenantEntity
{
    public Guid AssetVersionId { get; set; }
    public string? CoordinatesJson { get; set; }
    public string Body { get; set; } = string.Empty;
    public Guid? AuthorId { get; set; }
    public DateTime PostedAtUtc { get; set; } = DateTime.UtcNow;
}
