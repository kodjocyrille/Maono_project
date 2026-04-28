using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.DTOs;
using Maono.Application.Features.Campaigns.Queries;
using Maono.Domain.Campaigns.Repository;
using MediatR;

namespace Maono.Application.Features.Campaigns.Handlers;

public class GetCampaignByIdHandler : IRequestHandler<GetCampaignByIdQuery, Result<CampaignDetailDto>>
{
    private readonly ICampaignRepository _repo;

    public GetCampaignByIdHandler(ICampaignRepository repo) => _repo = repo;

    public async Task<Result<CampaignDetailDto>> Handle(GetCampaignByIdQuery request, CancellationToken ct)
    {
        var campaign = await _repo.GetWithKpisAsync(request.Id, ct);
        if (campaign == null) return Result.Failure<CampaignDetailDto>("Campagne introuvable.", "NOT_FOUND");

        var kpis = campaign.Kpis.Select(k => new CampaignKpiDto(k.Id, k.Name, k.TargetValue, k.Unit)).ToList();

        return Result.Success(new CampaignDetailDto(
            campaign.Id, campaign.Name, campaign.Description, campaign.Objective, campaign.Status,
            campaign.ClientOrganizationId, campaign.StartDate, campaign.EndDate,
            campaign.BudgetPlanned, campaign.BudgetSpent,
            campaign.TargetReach, campaign.TargetCtr, campaign.TargetConversions,
            campaign.TargetEngagementRate, campaign.TargetPlatforms,
            campaign.CreatedAtUtc, kpis));
    }
}

public class ListCampaignsHandler : IRequestHandler<ListCampaignsQuery, Result<List<CampaignSummaryDto>>>
{
    private readonly ICampaignRepository _repo;

    public ListCampaignsHandler(ICampaignRepository repo) => _repo = repo;

    public async Task<Result<List<CampaignSummaryDto>>> Handle(ListCampaignsQuery request, CancellationToken ct)
    {
        var campaigns = request.ClientId.HasValue
            ? await _repo.GetByClientAsync(request.ClientId.Value, ct)
            : await _repo.GetAllAsync(ct);

        var dtos = campaigns.Select(c => new CampaignSummaryDto(
            c.Id, c.Name, c.Status, c.EndDate)).ToList();

        return Result.Success(dtos);
    }
}
