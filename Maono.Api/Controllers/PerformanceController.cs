using Maono.Api.Common;
using Maono.Application.Features.Performance.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanViewAnalytics")]
[Tags("Analytics")]
public class PerformanceController : ControllerBase
{
    private readonly IMediator _mediator;
    public PerformanceController(IMediator mediator) => _mediator = mediator;

    [HttpGet("publications/{publicationId}")]
    public async Task<IActionResult> GetByPublication(Guid publicationId)
    {
        var result = await _mediator.Send(new GetPublicationPerformanceQuery(publicationId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("campaigns/{campaignId}")]
    public async Task<IActionResult> GetByCampaign(Guid campaignId)
    {
        var result = await _mediator.Send(new GetCampaignPerformanceQuery(campaignId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("campaigns/{campaignId}/summary")]
    public async Task<IActionResult> GetCampaignSummary(Guid campaignId)
    {
        var result = await _mediator.Send(new GetCampaignPerformanceSummaryQuery(campaignId));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    // ── ECR-027 — Report Export ────────────────────────────

    /// <summary>Generate a report export (CSV/PDF).</summary>
    [HttpPost("reports")]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
    {
        var result = await _mediator.Send(new Application.Features.Performance.CreateReportExportCommand(
            request.CampaignId, request.ClientOrganizationId, request.Format, request.PeriodStart, request.PeriodEnd));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Rapport généré"));
    }

    /// <summary>List report exports.</summary>
    [HttpGet("reports")]
    public async Task<IActionResult> ListReports()
    {
        var result = await _mediator.Send(new Application.Features.Performance.ListReportExportsQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    // ── ECR-028/029 — Freelance Score ──────────────────────

    /// <summary>Get performance score for a freelance user.</summary>
    [HttpGet("users/{userId}/score")]
    public async Task<IActionResult> GetFreelanceScore(Guid userId)
    {
        var result = await _mediator.Send(new Application.Features.Performance.GetFreelanceScoreQuery(userId));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }
}

public record CreateReportRequest(
    Guid? CampaignId,
    Guid? ClientOrganizationId,
    string Format,
    DateTime? PeriodStart,
    DateTime? PeriodEnd
);
