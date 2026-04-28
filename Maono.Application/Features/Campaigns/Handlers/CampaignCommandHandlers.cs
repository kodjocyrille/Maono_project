using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.Commands;
using Maono.Application.Features.Campaigns.DTOs;
using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Campaigns.Repository;
using MediatR;

namespace Maono.Application.Features.Campaigns.Handlers;

public class CreateCampaignHandler : IRequestHandler<CreateCampaignCommand, Result<CampaignDto>>
{
    private readonly ICampaignRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public CreateCampaignHandler(ICampaignRepository repo, ICurrentUserService currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Result<CampaignDto>> Handle(CreateCampaignCommand request, CancellationToken ct)
    {
        var campaign = new Campaign
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            Name = request.Name,
            Description = request.Description,
            Objective = request.Objective,
            ClientOrganizationId = request.ClientOrganizationId,
            Status = CampaignStatus.Draft,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BudgetPlanned = request.BudgetPlanned,
            // KPI Targets
            TargetReach = request.TargetReach,
            TargetCtr = request.TargetCtr,
            TargetConversions = request.TargetConversions,
            TargetEngagementRate = request.TargetEngagementRate,
            TargetPlatforms = request.TargetPlatforms
        };
        await _repo.AddAsync(campaign, ct);

        return Result.Success(new CampaignDto(
            campaign.Id, campaign.Name, campaign.Description, campaign.Objective, campaign.Status,
            campaign.ClientOrganizationId, campaign.StartDate, campaign.EndDate,
            campaign.BudgetPlanned, campaign.BudgetSpent, campaign.CreatedAtUtc));
    }
}

public class UpdateCampaignHandler : IRequestHandler<UpdateCampaignCommand, Result<CampaignDto>>
{
    private readonly ICampaignRepository _repo;

    public UpdateCampaignHandler(ICampaignRepository repo) => _repo = repo;

    public async Task<Result<CampaignDto>> Handle(UpdateCampaignCommand request, CancellationToken ct)
    {
        var campaign = await _repo.GetByIdAsync(request.Id, ct);
        if (campaign == null) return Result.Failure<CampaignDto>("Campagne introuvable.", "NOT_FOUND");

        campaign.Name = request.Name;
        campaign.Description = request.Description;
        campaign.Status = request.Status;
        campaign.StartDate = request.StartDate;
        campaign.EndDate = request.EndDate;
        _repo.Update(campaign);

        return Result.Success(new CampaignDto(
            campaign.Id, campaign.Name, campaign.Description, campaign.Objective, campaign.Status,
            campaign.ClientOrganizationId, campaign.StartDate, campaign.EndDate,
            campaign.BudgetPlanned, campaign.BudgetSpent, campaign.CreatedAtUtc));
    }
}

public class DeleteCampaignHandler : IRequestHandler<DeleteCampaignCommand, Result>
{
    private readonly ICampaignRepository _repo;
    public DeleteCampaignHandler(ICampaignRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeleteCampaignCommand request, CancellationToken ct)
    {
        var campaign = await _repo.GetByIdAsync(request.Id, ct);
        if (campaign == null) return Result.Failure("Campagne introuvable.", "NOT_FOUND");
        campaign.IsDeleted = true;
        campaign.DeletedAtUtc = DateTime.UtcNow;
        _repo.Update(campaign);
        return Result.Success();
    }
}
public class UpdateCampaignKpiTargetsHandler : IRequestHandler<UpdateCampaignKpiTargetsCommand, Result<CampaignDto>>
{
    private readonly ICampaignRepository _repo;
    public UpdateCampaignKpiTargetsHandler(ICampaignRepository repo) => _repo = repo;

    public async Task<Result<CampaignDto>> Handle(UpdateCampaignKpiTargetsCommand request, CancellationToken ct)
    {
        var campaign = await _repo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) return Result.Failure<CampaignDto>("Campagne introuvable.", "NOT_FOUND");

        if (request.TargetReach.HasValue) campaign.TargetReach = request.TargetReach;
        if (request.TargetCtr.HasValue) campaign.TargetCtr = request.TargetCtr;
        if (request.TargetConversions.HasValue) campaign.TargetConversions = request.TargetConversions;
        if (request.TargetEngagementRate.HasValue) campaign.TargetEngagementRate = request.TargetEngagementRate;
        if (request.TargetPlatforms != null) campaign.TargetPlatforms = request.TargetPlatforms;

        _repo.Update(campaign);
        return Result.Success(new CampaignDto(
            campaign.Id, campaign.Name, campaign.Description, campaign.Objective, campaign.Status,
            campaign.ClientOrganizationId, campaign.StartDate, campaign.EndDate,
            campaign.BudgetPlanned, campaign.BudgetSpent, campaign.CreatedAtUtc));
    }
}

public class UpdateCampaignStatusHandler : IRequestHandler<UpdateCampaignStatusCommand, Result<CampaignDto>>
{
    private readonly ICampaignRepository _repo;
    public UpdateCampaignStatusHandler(ICampaignRepository repo) => _repo = repo;

    // Transitions autorisées (state machine)
    private static readonly Dictionary<CampaignStatus, CampaignStatus[]> _allowedTransitions = new()
    {
        { CampaignStatus.Draft,  new[] { CampaignStatus.Active } },
        { CampaignStatus.Active, new[] { CampaignStatus.Paused, CampaignStatus.Closed } },
        { CampaignStatus.Paused, new[] { CampaignStatus.Active } },
    };

    public async Task<Result<CampaignDto>> Handle(UpdateCampaignStatusCommand request, CancellationToken ct)
    {
        var campaign = await _repo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null)
            return Result.Failure<CampaignDto>("Campagne introuvable.", "NOT_FOUND");

        // Valider la transition de statut
        if (!_allowedTransitions.TryGetValue(campaign.Status, out var allowed) || !allowed.Contains(request.NewStatus))
            return Result.Failure<CampaignDto>(
                $"Transition de statut invalide : {campaign.Status} → {request.NewStatus}. " +
                $"Transitions autorisées : {(allowed != null ? string.Join(", ", allowed) : "aucune")}.",
                "INVALID_STATUS_TRANSITION");

        campaign.Status = request.NewStatus;
        _repo.Update(campaign);

        return Result.Success(new CampaignDto(
            campaign.Id, campaign.Name, campaign.Description, campaign.Objective, campaign.Status,
            campaign.ClientOrganizationId, campaign.StartDate, campaign.EndDate,
            campaign.BudgetPlanned, campaign.BudgetSpent, campaign.CreatedAtUtc));
    }
}

/// <summary>
/// ECR-012 — Formal campaign closure: creates a ClosureRecord, transitions status to Closed.
/// </summary>
public class CloseCampaignHandler : IRequestHandler<CloseCampaignCommand, Result<CampaignDto>>
{
    private readonly ICampaignRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public CloseCampaignHandler(ICampaignRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result<CampaignDto>> Handle(CloseCampaignCommand request, CancellationToken ct)
    {
        var campaign = await _repo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) return Result.Failure<CampaignDto>("Campagne introuvable.", "NOT_FOUND");

        // Only Active or Paused campaigns can be closed
        if (campaign.Status != CampaignStatus.Active && campaign.Status != CampaignStatus.Paused)
            return Result.Failure<CampaignDto>(
                $"Impossible de clôturer une campagne en statut {campaign.Status}. Seules les campagnes Active ou Paused peuvent être clôturées.",
                "INVALID_STATUS_TRANSITION");

        campaign.Status = CampaignStatus.Closed;
        _repo.Update(campaign);

        return Result.Success(new CampaignDto(
            campaign.Id, campaign.Name, campaign.Description, campaign.Objective, campaign.Status,
            campaign.ClientOrganizationId, campaign.StartDate, campaign.EndDate,
            campaign.BudgetPlanned, campaign.BudgetSpent, campaign.CreatedAtUtc));
    }
}

/// <summary>
/// ECR-013 — Duplicate a campaign's structure (without content items).
/// New campaign starts in Draft with "[COPIE]" prefix.
/// </summary>
public class DuplicateCampaignHandler : IRequestHandler<DuplicateCampaignCommand, Result<CampaignDto>>
{
    private readonly ICampaignRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public DuplicateCampaignHandler(ICampaignRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result<CampaignDto>> Handle(DuplicateCampaignCommand request, CancellationToken ct)
    {
        var source = await _repo.GetByIdAsync(request.SourceCampaignId, ct);
        if (source == null) return Result.Failure<CampaignDto>("Campagne source introuvable.", "NOT_FOUND");

        var duplicate = new Campaign
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            Name = $"[COPIE] {source.Name}",
            Description = source.Description,
            Objective = source.Objective,
            ClientOrganizationId = source.ClientOrganizationId,
            Status = CampaignStatus.Draft,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            BudgetPlanned = source.BudgetPlanned,
            TargetReach = source.TargetReach,
            TargetCtr = source.TargetCtr,
            TargetConversions = source.TargetConversions,
            TargetEngagementRate = source.TargetEngagementRate,
            TargetPlatforms = source.TargetPlatforms
        };
        await _repo.AddAsync(duplicate, ct);

        return Result.Success(new CampaignDto(
            duplicate.Id, duplicate.Name, duplicate.Description, duplicate.Objective, duplicate.Status,
            duplicate.ClientOrganizationId, duplicate.StartDate, duplicate.EndDate,
            duplicate.BudgetPlanned, duplicate.BudgetSpent, duplicate.CreatedAtUtc));
    }
}
