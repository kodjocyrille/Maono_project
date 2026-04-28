using Maono.Domain.Notifications.Entities;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Publications.Entities;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Assets.Entities;
using Maono.Domain.Content.Entities;
using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Common;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Content.Enums;
using Maono.Domain.Assets.Enums;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Notifications.Enums;

namespace Maono.Domain.Assets.Entities;

/// <summary>
/// Asset aggregate root. Logical file with versioning support.
/// </summary>
public class Asset : TenantAggregateRoot
{
    public Guid ContentItemId { get; set; }
    public AssetType AssetType { get; set; }
    public int CurrentVersionNumber { get; set; } = 1;
    public string? CurrentStoragePath { get; set; }
    public string? MimeType { get; set; }
    public string? OriginalFileName { get; set; }
    public AssetVisibility Visibility { get; set; } = AssetVisibility.Internal;

    // Navigation
    public Content.Entities.ContentItem ContentItem { get; set; } = null!;
    public ICollection<AssetVersion> Versions { get; set; } = new List<AssetVersion>();
}
