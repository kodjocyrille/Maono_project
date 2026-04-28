using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Campaigns.Repository;
using Maono.Domain.Content.Repository;
using Maono.Domain.Content.Enums;
using Maono.Domain.Planning.Repository;
using MediatR;

namespace Maono.Application.Features.Campaigns.Queries;

/// <summary>
/// ECR-038 — Campaign progress dashboard with real-time indicators.
/// </summary>
public record GetCampaignDashboardQuery(Guid CampaignId) : IQuery<Result<CampaignDashboardDto>>;

public record CampaignDashboardDto(
    Guid CampaignId,
    string CampaignName,
    string Status,
    int TotalContents,
    int DraftCount,
    int InProductionCount,
    int InReviewCount,
    int PublishedCount,
    int ArchivedCount,
    decimal CompletionPercent,
    decimal? BudgetPlanned,
    decimal? BudgetSpent,
    DateTime? StartDate,
    DateTime? EndDate
);

public class GetCampaignDashboardHandler : IRequestHandler<GetCampaignDashboardQuery, Result<CampaignDashboardDto>>
{
    private readonly ICampaignRepository _campaignRepo;
    private readonly ICalendarRepository _calendarRepo;
    private readonly IContentRepository _contentRepo;

    public GetCampaignDashboardHandler(ICampaignRepository campaignRepo, ICalendarRepository calendarRepo, IContentRepository contentRepo)
    {
        _campaignRepo = campaignRepo;
        _calendarRepo = calendarRepo;
        _contentRepo = contentRepo;
    }

    public async Task<Result<CampaignDashboardDto>> Handle(GetCampaignDashboardQuery request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) return Result.Failure<CampaignDashboardDto>("Campagne introuvable.", "NOT_FOUND");

        // ContentItem links to Campaign via CalendarEntry
        var calendarEntries = await _calendarRepo.GetByCampaignAsync(request.CampaignId, ct);
        var entryIds = calendarEntries.Select(e => e.Id).ToHashSet();

        var contents = await _contentRepo.FindAsync(c => c.CalendarEntryId.HasValue && entryIds.Contains(c.CalendarEntryId.Value), ct);
        var total = contents.Count;
        var published = contents.Count(c => c.Status == ContentStatus.Published);
        var archived = contents.Count(c => c.Status == ContentStatus.Archived);
        var completion = total > 0 ? Math.Round((decimal)(published + archived) / total * 100, 1) : 0;

        return Result.Success(new CampaignDashboardDto(
            campaign.Id,
            campaign.Name,
            campaign.Status.ToString(),
            total,
            contents.Count(c => c.Status == ContentStatus.Draft),
            contents.Count(c => c.Status == ContentStatus.InProduction),
            contents.Count(c => c.Status == ContentStatus.InReview),
            published,
            archived,
            completion,
            campaign.BudgetPlanned,
            campaign.BudgetSpent,
            campaign.StartDate,
            campaign.EndDate));
    }
}
