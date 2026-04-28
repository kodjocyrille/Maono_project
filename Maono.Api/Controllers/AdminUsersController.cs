using Maono.Api.Common;
using Maono.Application.Features.Admin;
using Maono.Application.Features.Admin.Commands;
using Maono.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
[Tags("Administration")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminUsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? roleFilter,
        [FromQuery] bool activeOnly = true)
    {
        var result = await _mediator.Send(new ListUsersQuery(search, roleFilter, activeOnly));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPatch("{userId}")]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateUserRequest request)
    {
        var result = await _mediator.Send(new UpdateUserCommand(userId, request.RoleName, request.IsActive));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> Deactivate(Guid userId)
    {
        var result = await _mediator.Send(new DeactivateUserCommand(userId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse.Ok("Utilisateur désactivé"));
    }
}

public record UpdateUserRequest(string? RoleName, bool? IsActive);
