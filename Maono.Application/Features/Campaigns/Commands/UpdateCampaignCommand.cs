using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.DTOs;
using Maono.Domain.Campaigns.Enums;

namespace Maono.Application.Features.Campaigns.Commands;

public record UpdateCampaignCommand(
    Guid Id,
    string Name,
    string? Description,
    CampaignStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal? Budget
) : ICommand<Result<CampaignDto>>;
