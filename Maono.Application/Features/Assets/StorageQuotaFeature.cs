using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Assets.Repository;
using MediatR;

namespace Maono.Application.Features.Assets;

// ── ECR-031 — Storage Quota ─────────────────────────────

public record GetStorageQuotaQuery() : IQuery<Result<StorageQuotaDto>>;

public record StorageQuotaDto(
    long UsedBytes,
    long QuotaBytes,
    decimal UtilizationPercent,
    int TotalAssets,
    string Tier
);

public class GetStorageQuotaHandler : IRequestHandler<GetStorageQuotaQuery, Result<StorageQuotaDto>>
{
    private readonly IAssetRepository _assetRepo;
    public GetStorageQuotaHandler(IAssetRepository assetRepo) => _assetRepo = assetRepo;

    public async Task<Result<StorageQuotaDto>> Handle(GetStorageQuotaQuery request, CancellationToken ct)
    {
        var assets = await _assetRepo.GetAllAsync(ct);
        var totalAssets = assets.Count;

        // Sum file sizes from all versions
        long usedBytes = 0;
        foreach (var asset in assets)
        {
            var withVersions = await _assetRepo.GetWithVersionsAsync(asset.Id, ct);
            if (withVersions?.Versions != null)
                usedBytes += withVersions.Versions.Sum(v => v.FileSize);
        }

        // Tier-based quotas (configurable in future)
        const long freeQuota = 5L * 1024 * 1024 * 1024;   // 5 GB
        const long proQuota = 50L * 1024 * 1024 * 1024;    // 50 GB
        var quotaBytes = totalAssets > 100 ? proQuota : freeQuota;
        var utilization = quotaBytes > 0 ? Math.Round((decimal)usedBytes / quotaBytes * 100, 1) : 0;
        var tier = totalAssets > 100 ? "Pro" : "Free";

        return Result.Success(new StorageQuotaDto(usedBytes, quotaBytes, utilization, totalAssets, tier));
    }
}
