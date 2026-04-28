using Maono.Api.Common;
using Maono.Application.Features.Approvals.Commands;
using Maono.Application.Features.Approvals.Queries;
using Maono.Domain.Approval.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Contenus & Production")]
public class ApprovalsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ApprovalsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("internal")]
    [Authorize(Policy = "CanApprove")]
    public async Task<IActionResult> SubmitInternal([FromBody] SubmitInternalApprovalCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPost("client")]
    public async Task<IActionResult> SubmitClient([FromBody] SubmitClientApprovalCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("contents/{contentId}/cycles")]
    public async Task<IActionResult> GetCycles(Guid contentId)
    {
        var result = await _mediator.Send(new GetApprovalCyclesQuery(contentId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }
}
