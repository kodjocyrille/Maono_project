using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.DTOs;

namespace Maono.Application.Features.Campaigns.Queries;

public record GetCampaignByIdQuery(Guid Id) : IQuery<Result<CampaignDetailDto>>;

public record ListCampaignsQuery(Guid? ClientId = null, string? Status = null)
    : IQuery<Result<List<CampaignSummaryDto>>>;
