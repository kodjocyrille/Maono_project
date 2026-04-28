using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.Commands;
using Maono.Application.Features.Assets.DTOs;
using Maono.Application.Features.Assets.Queries;
using Maono.Domain.Assets.Repository;
using MediatR;

namespace Maono.Application.Features.Assets.Handlers;

public class GetAssetByIdHandler : IRequestHandler<GetAssetByIdQuery, Result<AssetDetailDto>>
{
    private readonly IAssetRepository _repo;
    private readonly IAssetStorageService _storage;

    public GetAssetByIdHandler(IAssetRepository repo, IAssetStorageService storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task<Result<AssetDetailDto>> Handle(GetAssetByIdQuery request, CancellationToken ct)
    {
        var asset = await _repo.GetWithVersionsAsync(request.Id, ct);
        if (asset == null) return Result.Failure<AssetDetailDto>("Asset not found", "NOT_FOUND");
        var versions = asset.Versions.Select(v => new AssetVersionDto(v.Id, v.VersionNumber, v.StoragePath, v.FileSize, v.UploadedBy, v.UploadedAtUtc)).ToList();

        // Générer une URL de téléchargement presigned (1h)
        string? downloadUrl = null;
        if (!string.IsNullOrEmpty(asset.CurrentStoragePath))
        {
            try
            {
                downloadUrl = await _storage.GetSignedUrlAsync(asset.CurrentStoragePath, TimeSpan.FromHours(1), ct);
            }
            catch { /* MinIO indisponible — on retourne null */ }
        }

        return Result.Success(new AssetDetailDto(asset.Id, asset.ContentItemId, asset.AssetType, asset.CurrentVersionNumber, asset.CurrentStoragePath, asset.MimeType, asset.OriginalFileName, asset.Visibility, downloadUrl, asset.CreatedAtUtc, versions));
    }
}

public class GetAssetVersionsHandler : IRequestHandler<GetAssetVersionsQuery, Result<List<AssetVersionDto>>>
{
    private readonly IAssetRepository _repo;
    public GetAssetVersionsHandler(IAssetRepository repo) => _repo = repo;

    public async Task<Result<List<AssetVersionDto>>> Handle(GetAssetVersionsQuery request, CancellationToken ct)
    {
        var asset = await _repo.GetWithVersionsAsync(request.AssetId, ct);
        if (asset == null) return Result.Failure<List<AssetVersionDto>>("Asset not found", "NOT_FOUND");
        var versions = asset.Versions.OrderByDescending(v => v.VersionNumber)
            .Select(v => new AssetVersionDto(v.Id, v.VersionNumber, v.StoragePath, v.FileSize, v.UploadedBy, v.UploadedAtUtc)).ToList();
        return Result.Success(versions);
    }
}

public class RestoreAssetVersionHandler : IRequestHandler<RestoreAssetVersionCommand, Result<AssetDto>>
{
    private readonly IAssetRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public RestoreAssetVersionHandler(IAssetRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result<AssetDto>> Handle(RestoreAssetVersionCommand request, CancellationToken ct)
    {
        var asset = await _repo.GetWithVersionsAsync(request.AssetId, ct);
        if (asset == null) return Result.Failure<AssetDto>("Asset not found", "NOT_FOUND");
        var version = asset.Versions.FirstOrDefault(v => v.VersionNumber == request.TargetVersionNumber);
        if (version == null) return Result.Failure<AssetDto>("Version not found", "NOT_FOUND");

        asset.CurrentVersionNumber = version.VersionNumber;
        asset.CurrentStoragePath = version.StoragePath;
        _repo.Update(asset);

        return Result.Success(new AssetDto(asset.Id, asset.ContentItemId, asset.AssetType, asset.CurrentVersionNumber, asset.MimeType, asset.OriginalFileName, null, asset.CreatedAtUtc));
    }
}
