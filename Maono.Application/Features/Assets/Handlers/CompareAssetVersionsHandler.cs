using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.DTOs;
using Maono.Application.Features.Assets.Queries;
using Maono.Domain.Assets.Repository;
using MediatR;

namespace Maono.Application.Features.Assets.Handlers;

/// <summary>
/// ECR-030 — Compare two versions of an asset side-by-side.
/// </summary>
public class CompareAssetVersionsHandler : IRequestHandler<CompareAssetVersionsQuery, Result<AssetVersionComparisonDto>>
{
    private readonly IAssetRepository _assetRepo;
    public CompareAssetVersionsHandler(IAssetRepository assetRepo) => _assetRepo = assetRepo;

    public async Task<Result<AssetVersionComparisonDto>> Handle(CompareAssetVersionsQuery request, CancellationToken ct)
    {
        var asset = await _assetRepo.GetWithVersionsAsync(request.AssetId, ct);
        if (asset == null) return Result.Failure<AssetVersionComparisonDto>("Asset introuvable.", "NOT_FOUND");

        var vA = asset.Versions.FirstOrDefault(v => v.VersionNumber == request.VersionA);
        var vB = asset.Versions.FirstOrDefault(v => v.VersionNumber == request.VersionB);

        if (vA == null) return Result.Failure<AssetVersionComparisonDto>($"Version {request.VersionA} introuvable.", "VERSION_NOT_FOUND");
        if (vB == null) return Result.Failure<AssetVersionComparisonDto>($"Version {request.VersionB} introuvable.", "VERSION_NOT_FOUND");

        return Result.Success(new AssetVersionComparisonDto(
            asset.Id,
            new AssetVersionDto(vA.Id, vA.VersionNumber, vA.StoragePath, vA.FileSize, vA.UploadedBy, vA.UploadedAtUtc),
            new AssetVersionDto(vB.Id, vB.VersionNumber, vB.StoragePath, vB.FileSize, vB.UploadedBy, vB.UploadedAtUtc)));
    }
}
