using Maono.Domain.Assets.Enums;

namespace Maono.Application.Features.Assets.DTOs;

public record AssetDto(Guid Id, Guid ContentItemId, AssetType AssetType, int CurrentVersionNumber, string? MimeType, string? OriginalFileName, string? DownloadUrl, DateTime CreatedAtUtc);
public record AssetDetailDto(Guid Id, Guid ContentItemId, AssetType AssetType, int CurrentVersionNumber, string? CurrentStoragePath, string? MimeType, string? OriginalFileName, AssetVisibility Visibility, string? DownloadUrl, DateTime CreatedAtUtc, List<AssetVersionDto> Versions);
public record AssetVersionDto(Guid Id, int VersionNumber, string StoragePath, long FileSize, string? UploadedBy, DateTime UploadedAtUtc);
