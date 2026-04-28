using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.Queries;
using Maono.Domain.Assets.Entities;
using Maono.Domain.Assets.Repository;
using MediatR;

namespace Maono.Application.Features.Assets.Handlers;

/// <summary>
/// ECR-020 — Search assets with filters (filename, type, contentItemId).
/// </summary>
public class SearchAssetsHandler : IRequestHandler<SearchAssetsQuery, Result<List<AssetSummaryDto>>>
{
    private readonly IAssetRepository _repo;
    public SearchAssetsHandler(IAssetRepository repo) => _repo = repo;

    public async Task<Result<List<AssetSummaryDto>>> Handle(SearchAssetsQuery request, CancellationToken ct)
    {
        // Start from all assets, apply filters progressively
        IReadOnlyList<Asset> assets;

        if (request.ContentItemId.HasValue)
        {
            assets = await _repo.GetByContentItemAsync(request.ContentItemId.Value, ct);
        }
        else
        {
            assets = await _repo.GetAllAsync(ct);
        }

        var filtered = assets.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            filtered = filtered.Where(a =>
                a.OriginalFileName != null &&
                a.OriginalFileName.Contains(request.FileName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.AssetType))
        {
            filtered = filtered.Where(a => a.AssetType.ToString().Equals(request.AssetType, StringComparison.OrdinalIgnoreCase));
        }

        var dtos = filtered.Select(a => new AssetSummaryDto(
            a.Id, a.ContentItemId, a.AssetType.ToString(), a.OriginalFileName, a.MimeType,
            a.CurrentVersionNumber, a.CreatedAtUtc)).ToList();

        return Result.Success(dtos);
    }
}
