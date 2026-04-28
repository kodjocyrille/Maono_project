using Maono.Api.Common;
using Maono.Application.Features.Messages.Commands;
using Maono.Application.Features.Messages.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Contenus & Production")]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    public MessagesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("contents/{contentId}")]
    public async Task<IActionResult> GetByContent(Guid contentId)
    {
        var result = await _mediator.Send(new GetContentMessagesQuery(contentId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPost("contents/{contentId}")]
    public async Task<IActionResult> Send(Guid contentId, [FromBody] SendMessageRequest request)
    {
        var result = await _mediator.Send(new SendContentMessageCommand(contentId, request.Body));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>ECR-008a — Export all messages for a campaign.</summary>
    [HttpGet("campaigns/{campaignId}/export")]
    public async Task<IActionResult> ExportByCampaign(Guid campaignId)
    {
        var result = await _mediator.Send(new ExportCampaignMessagesQuery(campaignId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }
}

public record SendMessageRequest(string Body);
