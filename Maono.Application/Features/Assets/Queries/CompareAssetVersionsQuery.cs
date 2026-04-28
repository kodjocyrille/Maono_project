using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.DTOs;

namespace Maono.Application.Features.Assets.Queries;

/// <summary>
/// ECR-030 — Compare two versions of an asset side-by-side.
/// Returns the metadata and storage paths for both versions.
/// </summary>
public record CompareAssetVersionsQuery(
    Guid AssetId,
    int VersionA,
    int VersionB
) : IQuery<Result<AssetVersionComparisonDto>>;

public record AssetVersionComparisonDto(
    Guid AssetId,
    AssetVersionDto VersionA,
    AssetVersionDto VersionB
);
