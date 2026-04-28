using Maono.Domain.Content.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Content.Entities;

public class Brief : TenantEntity
{
    public Guid ContentItemId { get; set; }
    public string Body { get; set; } = string.Empty;

    // Navigation
    public ContentItem ContentItem { get; set; } = null!;
}
