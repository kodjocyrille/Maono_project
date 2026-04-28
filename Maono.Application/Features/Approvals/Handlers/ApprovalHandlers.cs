using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Approvals.Commands;
using Maono.Application.Features.Approvals.DTOs;
using Maono.Application.Features.Approvals.Queries;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Approval.Repository;
using MediatR;

namespace Maono.Application.Features.Approvals.Handlers;

public class SubmitInternalApprovalHandler : IRequestHandler<SubmitInternalApprovalCommand, Result<ApprovalDecisionDto>>
{
    private readonly IApprovalRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public SubmitInternalApprovalHandler(IApprovalRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result<ApprovalDecisionDto>> Handle(SubmitInternalApprovalCommand request, CancellationToken ct)
    {
        var cycles = await _repo.GetByContentAsync(request.ContentItemId, ct);
        var cycle = cycles.OrderByDescending(c => c.RevisionRound).FirstOrDefault();
        if (cycle == null) return Result.Failure<ApprovalDecisionDto>("No approval cycle found", "NOT_FOUND");

        // Need full cycle with decisions
        var full = await _repo.GetWithDecisionsAsync(cycle.Id, ct);
        if (full == null) return Result.Failure<ApprovalDecisionDto>("Cycle not found", "NOT_FOUND");

        var decision = new ApprovalDecision
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            ApprovalCycleId = full.Id,
            ActorType = ActorType.InternalUser,
            ActorId = _currentUser.UserId,
            Decision = request.Decision,
            Comment = request.Comment,
        };
        full.Decisions.Add(decision);
        full.InternalStatus = request.Decision;
        _repo.Update(full);

        return Result.Success(new ApprovalDecisionDto(decision.Id, decision.ActorType, decision.ActorId, decision.Decision, decision.Comment, decision.DecidedAtUtc));
    }
}

/// <summary>
/// ECR-003 — Client approval is only allowed when InternalStatus == Approved.
/// ECR-004 — Comment is mandatory when the client rejects.
/// </summary>
public class SubmitClientApprovalHandler : IRequestHandler<SubmitClientApprovalCommand, Result<ApprovalDecisionDto>>
{
    private readonly IApprovalRepository _repo;
    private readonly ICurrentUserService _currentUser;

    public SubmitClientApprovalHandler(IApprovalRepository repo, ICurrentUserService currentUser) { _repo = repo; _currentUser = currentUser; }

    public async Task<Result<ApprovalDecisionDto>> Handle(SubmitClientApprovalCommand request, CancellationToken ct)
    {
        var cycles = await _repo.GetByContentAsync(request.ContentItemId, ct);
        var cycle = cycles.OrderByDescending(c => c.RevisionRound).FirstOrDefault();
        if (cycle == null) return Result.Failure<ApprovalDecisionDto>("No approval cycle found", "NOT_FOUND");

        var full = await _repo.GetWithDecisionsAsync(cycle.Id, ct);
        if (full == null) return Result.Failure<ApprovalDecisionDto>("Cycle not found", "NOT_FOUND");

        // ECR-003: Client review requires prior internal approval
        if (full.InternalStatus != ApprovalStatus.Approved)
            return Result.Failure<ApprovalDecisionDto>(
                "L'approbation interne doit être validée avant la revue client.",
                "INTERNAL_APPROVAL_REQUIRED");

        // ECR-004: Comment is mandatory on client rejection
        if (request.Decision == ApprovalStatus.Rejected && string.IsNullOrWhiteSpace(request.Comment))
            return Result.Failure<ApprovalDecisionDto>(
                "Un commentaire est obligatoire en cas de rejet.",
                "COMMENT_REQUIRED");

        var decision = new ApprovalDecision
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            ApprovalCycleId = full.Id,
            ActorType = ActorType.ClientContact,
            ActorId = _currentUser.UserId,
            Decision = request.Decision,
            Comment = request.Comment,
        };
        full.Decisions.Add(decision);
        full.ClientStatus = request.Decision;
        _repo.Update(full);

        return Result.Success(new ApprovalDecisionDto(decision.Id, decision.ActorType, decision.ActorId, decision.Decision, decision.Comment, decision.DecidedAtUtc));
    }
}

public class GetApprovalCyclesHandler : IRequestHandler<GetApprovalCyclesQuery, Result<List<ApprovalCycleDetailDto>>>
{
    private readonly IApprovalRepository _repo;
    public GetApprovalCyclesHandler(IApprovalRepository repo) => _repo = repo;

    public async Task<Result<List<ApprovalCycleDetailDto>>> Handle(GetApprovalCyclesQuery request, CancellationToken ct)
    {
        var cycles = await _repo.GetByContentAsync(request.ContentItemId, ct);
        var result = new List<ApprovalCycleDetailDto>();
        foreach (var c in cycles)
        {
            var full = await _repo.GetWithDecisionsAsync(c.Id, ct);
            if (full == null) continue;
            result.Add(new ApprovalCycleDetailDto(
                full.Id, full.ContentItemId, full.RevisionRound, full.InternalStatus, full.ClientStatus, full.StartedAtUtc, full.CompletedAtUtc,
                full.Decisions.Select(d => new ApprovalDecisionDto(d.Id, d.ActorType, d.ActorId, d.Decision, d.Comment, d.DecidedAtUtc)).ToList()));
        }
        return Result.Success(result);
    }
}
