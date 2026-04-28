using Maono.Domain.Content.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Content.Entities;

public class ContentDependency : TenantEntity
{
    public Guid SourceContentId { get; set; }
    public Guid BlockingContentId { get; set; }
    public string? DependencyType { get; set; }

    // Navigation
    public ContentItem SourceContent { get; set; } = null!;
    public ContentItem BlockingContent { get; set; } = null!;
}
