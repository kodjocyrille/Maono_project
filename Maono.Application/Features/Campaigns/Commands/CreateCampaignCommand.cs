using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.DTOs;

namespace Maono.Application.Features.Campaigns.Commands;

public record CreateCampaignCommand(
    string Name,
    string? Description,
    string? Objective,
    Guid ClientOrganizationId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? BudgetPlanned,
    // KPI Targets — optionnels à la création
    long? TargetReach,
    decimal? TargetCtr,
    long? TargetConversions,
    decimal? TargetEngagementRate,
    string[]? TargetPlatforms
) : ICommand<Result<CampaignDto>>;

