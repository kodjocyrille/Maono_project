using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Campaigns.Commands;

public record DeleteCampaignCommand(Guid Id) : ICommand<Result>;
