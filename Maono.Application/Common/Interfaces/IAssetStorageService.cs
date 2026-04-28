namespace Maono.Application.Common.Interfaces;

/// <summary>
/// S3-compatible asset storage abstraction.
/// </summary>
public interface IAssetStorageService
{
    // Legacy direct upload (kept for backward compatibility)
    Task<string> UploadAsync(Guid workspaceId, Guid assetId, int version, string fileName, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string storagePath, CancellationToken ct = default);
    Task<string> GetSignedUrlAsync(string storagePath, TimeSpan expiry, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);

    // Presigned URL flow (P1 — direct client-to-MinIO upload)
    Task<PresignedPutResult> GeneratePresignedPutUrlAsync(string storageKey, string mimeType, TimeSpan ttl, CancellationToken ct = default);
    Task<StorageObjectMetadata?> GetObjectMetadataAsync(string storageKey, CancellationToken ct = default);
}

public record PresignedPutResult(string Url, DateTime ExpiresAt);
public record StorageObjectMetadata(long SizeBytes, string? ChecksumSha256, string ETag);
