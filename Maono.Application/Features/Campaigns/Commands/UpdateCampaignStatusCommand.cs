using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.DTOs;
using Maono.Domain.Campaigns.Enums;

namespace Maono.Application.Features.Campaigns.Commands;

public record UpdateCampaignStatusCommand(
    Guid CampaignId,
    CampaignStatus NewStatus
) : ICommand<Result<CampaignDto>>;
