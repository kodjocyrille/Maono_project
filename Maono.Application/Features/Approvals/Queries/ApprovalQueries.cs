using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Approvals.DTOs;

namespace Maono.Application.Features.Approvals.Queries;

public record GetApprovalCyclesQuery(Guid ContentItemId) : IQuery<Result<List<ApprovalCycleDetailDto>>>;
