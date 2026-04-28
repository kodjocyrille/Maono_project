using Maono.Api.Common;
using Maono.Application.Features.Campaigns.Commands;
using Maono.Application.Features.Campaigns.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Campagnes")]
public class CampaignsController : ControllerBase
{
    private readonly IMediator _mediator;
    public CampaignsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Create([FromBody] CreateCampaignCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Campagne créée"));
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? clientId)
    {
        var result = await _mediator.Send(new ListCampaignsQuery(clientId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetCampaignByIdQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Error("L'identifiant ne correspond pas.", 400));
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteCampaignCommand(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Campagne archivée"));
    }

    [HttpPatch("{id}/kpi-targets")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> UpdateKpiTargets(Guid id, [FromBody] UpdateCampaignKpiTargetsCommand command)
    {
        var result = await _mediator.Send(command with { CampaignId = id });
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!, "KPI cibles mis à jour"));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCampaignStatusRequest request)
    {
        var result = await _mediator.Send(new UpdateCampaignStatusCommand(id, request.NewStatus));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Statut de la campagne mis à jour"));
    }

    /// <summary>ECR-012 — Clôture formelle avec archivage.</summary>
    [HttpPost("{id}/close")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseCampaignRequest request)
    {
        var result = await _mediator.Send(new CloseCampaignCommand(id, request.Summary));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!, "Campagne clôturée"));
    }

    /// <summary>ECR-013 — Duplication de campagne (structure sans contenus).</summary>
    [HttpPost("{id}/duplicate")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Duplicate(Guid id)
    {
        var result = await _mediator.Send(new DuplicateCampaignCommand(id));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Campagne dupliquée"));
    }

    // ── ECR-014 — Budget & Expenses ─────────────────────────

    /// <summary>Add an expense to a campaign.</summary>
    [HttpPost("{id}/expenses")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> AddExpense(Guid id, [FromBody] AddExpenseRequest request)
    {
        var result = await _mediator.Send(new AddCampaignExpenseCommand(id, request.Label, request.Amount, request.Category, request.Notes));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Dépense enregistrée"));
    }

    /// <summary>List all expenses for a campaign.</summary>
    [HttpGet("{id}/expenses")]
    public async Task<IActionResult> ListExpenses(Guid id)
    {
        var result = await _mediator.Send(new ListCampaignExpensesQuery(id));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>Budget summary: planned vs actual.</summary>
    [HttpGet("{id}/budget")]
    public async Task<IActionResult> GetBudget(Guid id)
    {
        var result = await _mediator.Send(new GetCampaignBudgetQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>ECR-038 — Campaign progress dashboard.</summary>
    [HttpGet("{id}/dashboard")]
    public async Task<IActionResult> GetDashboard(Guid id)
    {
        var result = await _mediator.Send(new GetCampaignDashboardQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }
}

public record UpdateCampaignStatusRequest(Maono.Domain.Campaigns.Enums.CampaignStatus NewStatus);
public record CloseCampaignRequest(string? Summary);
public record AddExpenseRequest(string Label, decimal Amount, string? Category, string? Notes);
