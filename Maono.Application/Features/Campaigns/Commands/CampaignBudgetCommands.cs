using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Campaigns.Commands;

/// <summary>
/// ECR-014 — Record an expense against a campaign budget.
/// Updates Campaign.BudgetSpent automatically.
/// </summary>
public record AddCampaignExpenseCommand(
    Guid CampaignId,
    string Label,
    decimal Amount,
    string? Category,
    string? Notes
) : ICommand<Result<CampaignExpenseDto>>;

public record ListCampaignExpensesQuery(Guid CampaignId) : IQuery<Result<List<CampaignExpenseDto>>>;

public record CampaignExpenseDto(
    Guid Id,
    Guid CampaignId,
    string Label,
    decimal Amount,
    string? Category,
    DateTime ExpenseDateUtc,
    string? Notes
);

/// <summary>
/// ECR-014 — Budget summary showing planned vs actual spending.
/// </summary>
public record GetCampaignBudgetQuery(Guid CampaignId) : IQuery<Result<CampaignBudgetDto>>;

public record CampaignBudgetDto(
    Guid CampaignId,
    decimal? BudgetPlanned,
    decimal BudgetSpent,
    decimal? BudgetRemaining,
    decimal UtilizationPercent,
    int ExpenseCount
);
