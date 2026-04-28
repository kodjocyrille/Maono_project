using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.DTOs;

namespace Maono.Application.Features.Campaigns.Commands;

public record UpdateCampaignKpiTargetsCommand(
    Guid CampaignId,
    long? TargetReach,
    decimal? TargetCtr,
    long? TargetConversions,
    decimal? TargetEngagementRate,
    string[]? TargetPlatforms
) : ICommand<Result<CampaignDto>>;
