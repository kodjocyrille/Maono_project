using Maono.Domain.Campaigns.Enums;

namespace Maono.Application.Features.Campaigns.DTOs;

public record CampaignDto(
    Guid Id,
    string Name,
    string? Description,
    string? Objective,
    CampaignStatus Status,
    Guid ClientOrganizationId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? BudgetPlanned,
    decimal? BudgetSpent,
    DateTime CreatedAtUtc
);

public record CampaignSummaryDto(
    Guid Id,
    string Name,
    CampaignStatus Status,
    DateTime? EndDate
);

public record CampaignDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string? Objective,
    CampaignStatus Status,
    Guid ClientOrganizationId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? BudgetPlanned,
    decimal? BudgetSpent,
    // KPI Targets (scalaires P7)
    long? TargetReach,
    decimal? TargetCtr,
    long? TargetConversions,
    decimal? TargetEngagementRate,
    string[]? TargetPlatforms,
    DateTime CreatedAtUtc,
    List<CampaignKpiDto> Kpis
);

public record CampaignKpiDto(Guid Id, string Name, decimal? TargetValue, string? Unit);
