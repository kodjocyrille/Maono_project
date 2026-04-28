using Maono.Domain.Content.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Content.Entities;

public class TaskChecklistItem : TenantEntity
{
    public Guid ContentItemId { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string? CompletedBy { get; set; }
    public DateTime? CompletedAtUtc { get; set; }

    // Navigation
    public ContentItem ContentItem { get; set; } = null!;
}
