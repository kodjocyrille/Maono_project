using Maono.Domain.Common;

namespace Maono.Domain.Assets.Entities;

/// <summary>
/// Tracks a pending direct-to-MinIO upload session.
/// Client uploads directly to presigned URL; API validates SHA-256 + size on confirmation.
/// </summary>
public class AssetUploadSession : TenantEntity
{
    public Guid ContentItemId { get; set; }
    public Guid InitiatedByUserId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long DeclaredSizeBytes { get; set; }
    public string DeclaredSha256 { get; set; } = string.Empty;   // hex, 64 chars

    public string StorageKey { get; set; } = string.Empty;        // path in MinIO
    public string PresignedUrl { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }

    public UploadSessionStatus Status { get; set; } = UploadSessionStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
}

public enum UploadSessionStatus
{
    Pending = 0,
    Confirmed = 1,
    Expired = 2,
    Failed = 3
}
