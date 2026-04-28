using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Approvals.DTOs;
using Maono.Domain.Approval.Enums;

namespace Maono.Application.Features.Approvals.Commands;

public record SubmitInternalApprovalCommand(Guid ContentItemId, ApprovalStatus Decision, string? Comment) : ICommand<Result<ApprovalDecisionDto>>;
public record SubmitClientApprovalCommand(Guid ContentItemId, ApprovalStatus Decision, string? Comment) : ICommand<Result<ApprovalDecisionDto>>;
