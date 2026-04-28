using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Campaigns.Commands;
using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Campaigns.Repository;
using Maono.Domain.Common;
using MediatR;

namespace Maono.Application.Features.Campaigns.Handlers;

/// <summary>
/// ECR-014 — Add an expense to a campaign and update BudgetSpent.
/// </summary>
public class AddCampaignExpenseHandler : IRequestHandler<AddCampaignExpenseCommand, Result<CampaignExpenseDto>>
{
    private readonly ICampaignRepository _campaignRepo;
    private readonly IGenericRepository<CampaignExpense> _expenseRepo;
    private readonly ICurrentUserService _currentUser;

    public AddCampaignExpenseHandler(ICampaignRepository campaignRepo, IGenericRepository<CampaignExpense> expenseRepo, ICurrentUserService currentUser)
    {
        _campaignRepo = campaignRepo;
        _expenseRepo = expenseRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<CampaignExpenseDto>> Handle(AddCampaignExpenseCommand request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) return Result.Failure<CampaignExpenseDto>("Campagne introuvable.", "NOT_FOUND");

        if (request.Amount <= 0)
            return Result.Failure<CampaignExpenseDto>("Le montant doit être positif.", "INVALID_AMOUNT");

        var expense = new CampaignExpense
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            CampaignId = request.CampaignId,
            Label = request.Label,
            Amount = request.Amount,
            Category = request.Category,
            Notes = request.Notes,
            RecordedBy = _currentUser.UserId?.ToString(),
            ExpenseDateUtc = DateTime.UtcNow
        };
        await _expenseRepo.AddAsync(expense, ct);

        // Update campaign's total spent
        campaign.BudgetSpent = (campaign.BudgetSpent ?? 0) + request.Amount;
        _campaignRepo.Update(campaign);

        return Result.Success(new CampaignExpenseDto(
            expense.Id, expense.CampaignId, expense.Label, expense.Amount,
            expense.Category, expense.ExpenseDateUtc, expense.Notes));
    }
}

/// <summary>
/// ECR-014 — List all expenses for a campaign.
/// </summary>
public class ListCampaignExpensesHandler : IRequestHandler<ListCampaignExpensesQuery, Result<List<CampaignExpenseDto>>>
{
    private readonly IGenericRepository<CampaignExpense> _expenseRepo;
    public ListCampaignExpensesHandler(IGenericRepository<CampaignExpense> expenseRepo) => _expenseRepo = expenseRepo;

    public async Task<Result<List<CampaignExpenseDto>>> Handle(ListCampaignExpensesQuery request, CancellationToken ct)
    {
        var expenses = await _expenseRepo.FindAsync(e => e.CampaignId == request.CampaignId, ct);
        var dtos = expenses.Select(e => new CampaignExpenseDto(
            e.Id, e.CampaignId, e.Label, e.Amount, e.Category, e.ExpenseDateUtc, e.Notes)).ToList();
        return Result.Success(dtos);
    }
}

/// <summary>
/// ECR-014 — Budget summary: planned vs actual.
/// </summary>
public class GetCampaignBudgetHandler : IRequestHandler<GetCampaignBudgetQuery, Result<CampaignBudgetDto>>
{
    private readonly ICampaignRepository _campaignRepo;
    private readonly IGenericRepository<CampaignExpense> _expenseRepo;

    public GetCampaignBudgetHandler(ICampaignRepository campaignRepo, IGenericRepository<CampaignExpense> expenseRepo)
    {
        _campaignRepo = campaignRepo;
        _expenseRepo = expenseRepo;
    }

    public async Task<Result<CampaignBudgetDto>> Handle(GetCampaignBudgetQuery request, CancellationToken ct)
    {
        var campaign = await _campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign == null) return Result.Failure<CampaignBudgetDto>("Campagne introuvable.", "NOT_FOUND");

        var expenses = await _expenseRepo.FindAsync(e => e.CampaignId == request.CampaignId, ct);
        var totalSpent = expenses.Sum(e => e.Amount);
        var remaining = campaign.BudgetPlanned.HasValue ? campaign.BudgetPlanned.Value - totalSpent : (decimal?)null;
        var utilization = campaign.BudgetPlanned.HasValue && campaign.BudgetPlanned.Value > 0
            ? (totalSpent / campaign.BudgetPlanned.Value) * 100m
            : 0m;

        return Result.Success(new CampaignBudgetDto(
            campaign.Id, campaign.BudgetPlanned, totalSpent, remaining, Math.Round(utilization, 1), expenses.Count));
    }
}
