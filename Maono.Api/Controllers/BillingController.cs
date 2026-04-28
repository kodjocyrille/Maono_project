using Maono.Api.Common;
using Maono.Application.Features.Billing;
using Maono.Domain.Missions.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

/// <summary>
/// ECR-023 — Facturation & bons de livraison.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Facturation")]
public class BillingController : ControllerBase
{
    private readonly IMediator _mediator;
    public BillingController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a billing record for a mission.</summary>
    [HttpPost]
    [Authorize(Policy = "CanManageMissions")]
    public async Task<IActionResult> Create([FromBody] CreateBillingRequest request)
    {
        var result = await _mediator.Send(new CreateBillingRecordCommand(
            request.MissionId, request.Amount, request.Currency, request.Notes));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Facture créée"));
    }

    /// <summary>List billing records, optionally filtered by mission.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? missionId)
    {
        var result = await _mediator.Send(new ListBillingRecordsQuery(missionId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>Update billing status (Draft → Sent → Paid).</summary>
    [HttpPatch("{id}/status")]
    [Authorize(Policy = "CanManageMissions")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBillingStatusRequest request)
    {
        var result = await _mediator.Send(new UpdateBillingStatusCommand(id, request.NewStatus));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Statut facturation mis à jour"));
    }

    /// <summary>Generate a delivery note for a mission delivery.</summary>
    [HttpPost("delivery-notes")]
    [Authorize(Policy = "CanManageMissions")]
    public async Task<IActionResult> GenerateDeliveryNote([FromBody] GenerateDeliveryNoteRequest request)
    {
        var result = await _mediator.Send(new GenerateDeliveryNoteCommand(request.MissionDeliveryId, request.Reference));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Bon de livraison généré"));
    }
}

public record CreateBillingRequest(Guid MissionId, decimal Amount, string? Currency, string? Notes);
public record UpdateBillingStatusRequest(BillingStatus NewStatus);
public record GenerateDeliveryNoteRequest(Guid MissionDeliveryId, string? Reference);
