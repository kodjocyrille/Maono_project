using Maono.Domain.Assets.Entities;
using Maono.Domain.Common;

namespace Maono.Domain.Assets.Entities;

public class AssetRestoreRecord : TenantEntity
{
    public Guid AssetId { get; set; }
    public int RestoredFromVersion { get; set; }
    public int RestoredToVersion { get; set; }
    public string? RestoredBy { get; set; }
    public DateTime RestoredAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public Asset Asset { get; set; } = null!;
}
