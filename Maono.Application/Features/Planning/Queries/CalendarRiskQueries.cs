using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using MediatR;

namespace Maono.Application.Features.Planning.Queries;

/// <summary>
/// ECR-021 — Calendar risk indicators (late content, pending approvals).
/// </summary>
public record GetCalendarRisksQuery() : IQuery<Result<CalendarRiskReport>>;

public record CalendarRiskReport(
    int OverdueContents,
    int PendingApprovals,
    int OverloadedResources,
    List<RiskItem> Items
);

public record RiskItem(
    string RiskType,
    Guid EntityId,
    string EntityName,
    string Description,
    DateTime? Deadline
);
