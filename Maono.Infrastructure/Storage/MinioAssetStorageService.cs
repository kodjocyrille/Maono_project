using Amazon.S3;
using Amazon.S3.Model;
using Maono.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Maono.Infrastructure.Storage;

public class MinioStorageSettings
{
    public const string SectionName = "MinioStorage";
    public string ServiceUrl { get; set; } = "http://localhost:9000";
    public string AccessKey { get; set; } = "maono-dev";
    public string SecretKey { get; set; } = "maono-dev-secret";
    public string BucketName { get; set; } = "maono-assets";
}

public class MinioAssetStorageService : IAssetStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly MinioStorageSettings _settings;

    public MinioAssetStorageService(IAmazonS3 s3Client, IOptions<MinioStorageSettings> settings)
    {
        _s3Client = s3Client;
        _settings = settings.Value;
    }

    public async Task<string> UploadAsync(Guid workspaceId, Guid assetId, int version, string fileName,
        Stream content, string contentType, CancellationToken ct = default)
    {
        await EnsureBucketExistsAsync(ct);

        var key = BuildKey(workspaceId, assetId, version, fileName);

        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request, ct);
        return key;
    }

    public async Task<Stream> DownloadAsync(string storagePath, CancellationToken ct = default)
    {
        var response = await _s3Client.GetObjectAsync(_settings.BucketName, storagePath, ct);
        return response.ResponseStream;
    }

    public Task<string> GetSignedUrlAsync(string storagePath, TimeSpan expiry, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = storagePath,
            Expires = DateTime.UtcNow.Add(expiry),
            Verb = HttpVerb.GET
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(url);
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        await _s3Client.DeleteObjectAsync(_settings.BucketName, storagePath, ct);
    }

    public async Task<PresignedPutResult> GeneratePresignedPutUrlAsync(
        string storageKey, string mimeType, TimeSpan ttl, CancellationToken ct = default)
    {
        // Garantit que le bucket existe avant toute opération
        await EnsureBucketExistsAsync(ct);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = storageKey,
            Expires = DateTime.UtcNow.Add(ttl),
            Verb = HttpVerb.PUT,
            ContentType = mimeType
        };
        var url = _s3Client.GetPreSignedURL(request);
        return new PresignedPutResult(url, DateTime.UtcNow.Add(ttl));
    }

    public async Task<StorageObjectMetadata?> GetObjectMetadataAsync(
        string storageKey, CancellationToken ct = default)
    {
        try
        {
            var response = await _s3Client.GetObjectMetadataAsync(_settings.BucketName, storageKey, ct);
            var checksum = response.Metadata.Keys.Contains("x-amz-checksum-sha256")
                ? response.Metadata["x-amz-checksum-sha256"]
                : null;
            return new StorageObjectMetadata(response.ContentLength, checksum, response.ETag);
        }
        catch (Amazon.S3.AmazonS3Exception ex) when (
            ex.StatusCode == System.Net.HttpStatusCode.NotFound ||
            ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            // NotFound  = objet absent dans le bucket
            // Forbidden = bucket inexistant (comportement MinIO)
            return null;
        }
    }

    private static string BuildKey(Guid workspaceId, Guid assetId, int version, string fileName)
    {
        return $"{workspaceId}/{assetId}/v{version}/{fileName}";
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        try
        {
            await _s3Client.EnsureBucketExistsAsync(_settings.BucketName);
        }
        catch
        {
            // Bucket already exists or creation is not supported (will fail on first upload)
        }
    }
}
