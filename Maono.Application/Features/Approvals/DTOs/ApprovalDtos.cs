using Maono.Domain.Approval.Enums;

namespace Maono.Application.Features.Approvals.DTOs;

public record ApprovalCycleDto(Guid Id, Guid ContentItemId, int RevisionRound, ApprovalStatus InternalStatus, ApprovalStatus ClientStatus, DateTime StartedAtUtc, DateTime? CompletedAtUtc);
public record ApprovalCycleDetailDto(Guid Id, Guid ContentItemId, int RevisionRound, ApprovalStatus InternalStatus, ApprovalStatus ClientStatus, DateTime StartedAtUtc, DateTime? CompletedAtUtc, List<ApprovalDecisionDto> Decisions);
public record ApprovalDecisionDto(Guid Id, ActorType ActorType, Guid? ActorId, ApprovalStatus Decision, string? Comment, DateTime DecidedAtUtc);
