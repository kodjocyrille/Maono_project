using Maono.Domain.Assets.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Assets.Entities;

public class AssetPreview : TenantEntity
{
    public Guid AssetVersionId { get; set; }
    public string PreviewPath { get; set; } = string.Empty;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public double? Duration { get; set; }

    // Navigation
    public AssetVersion AssetVersion { get; set; } = null!;
}
