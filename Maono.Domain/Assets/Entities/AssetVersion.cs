using Maono.Domain.Assets.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Assets.Entities;

public class AssetVersion : TenantEntity
{
    public Guid AssetId { get; set; }
    public int VersionNumber { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Checksum { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Asset Asset { get; set; } = null!;
    public AssetPreview? Preview { get; set; }
}
