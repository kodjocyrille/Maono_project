using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.DTOs;

namespace Maono.Application.Features.Campaigns.Commands;

/// <summary>
/// ECR-012 — Formal campaign closure with archival of related content.
/// </summary>
public record CloseCampaignCommand(Guid CampaignId, string? Summary) : ICommand<Result<CampaignDto>>;

/// <summary>
/// ECR-013 — Duplicate a campaign's structure (name, KPIs, tags, calendar) without content items.
/// </summary>
public record DuplicateCampaignCommand(Guid SourceCampaignId) : ICommand<Result<CampaignDto>>;
