using Maono.Application.Common.Models;
using Maono.Application.Features.Performance.DTOs;
using Maono.Application.Features.Performance.Queries;
using Maono.Domain.Performance.Repository;
using MediatR;

namespace Maono.Application.Features.Performance.Handlers;

public class GetPublicationPerformanceHandler : IRequestHandler<GetPublicationPerformanceQuery, Result<List<PerformanceSnapshotDto>>>
{
    private readonly IPerformanceRepository _repo;
    public GetPublicationPerformanceHandler(IPerformanceRepository repo) => _repo = repo;

    public async Task<Result<List<PerformanceSnapshotDto>>> Handle(GetPublicationPerformanceQuery request, CancellationToken ct)
    {
        var snapshots = await _repo.GetByPublicationAsync(request.PublicationId, ct);
        var dtos = snapshots.Select(s => new PerformanceSnapshotDto(s.Id, s.PublicationId, s.ContentItemId, s.CollectedAtUtc, s.Impressions, s.Reach, s.Engagement, s.Clicks, s.ConversionRate)).ToList();
        return Result.Success(dtos);
    }
}

public class GetCampaignPerformanceHandler : IRequestHandler<GetCampaignPerformanceQuery, Result<List<CampaignPerformanceSummaryDto>>>
{
    private readonly IPerformanceRepository _repo;
    public GetCampaignPerformanceHandler(IPerformanceRepository repo) => _repo = repo;

    public async Task<Result<List<CampaignPerformanceSummaryDto>>> Handle(GetCampaignPerformanceQuery request, CancellationToken ct)
    {
        var aggs = await _repo.GetByCampaignAsync(request.CampaignId, ct);
        var dtos = aggs.Select(a => new CampaignPerformanceSummaryDto(a.CampaignId, a.TotalImpressions, a.TotalReach, a.TotalEngagement, a.TotalClicks, a.AverageConversionRate)).ToList();
        return Result.Success(dtos);
    }
}

public class GetCampaignPerformanceSummaryHandler : IRequestHandler<GetCampaignPerformanceSummaryQuery, Result<CampaignPerformanceSummaryDto>>
{
    private readonly IPerformanceRepository _repo;
    public GetCampaignPerformanceSummaryHandler(IPerformanceRepository repo) => _repo = repo;

    public async Task<Result<CampaignPerformanceSummaryDto>> Handle(GetCampaignPerformanceSummaryQuery request, CancellationToken ct)
    {
        var aggs = await _repo.GetByCampaignAsync(request.CampaignId, ct);
        var latest = aggs.OrderByDescending(a => a.ComputedAtUtc).FirstOrDefault();
        if (latest == null) return Result.Failure<CampaignPerformanceSummaryDto>("No performance data", "NOT_FOUND");
        return Result.Success(new CampaignPerformanceSummaryDto(latest.CampaignId, latest.TotalImpressions, latest.TotalReach, latest.TotalEngagement, latest.TotalClicks, latest.AverageConversionRate));
    }
}
