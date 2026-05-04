using Maono.Api.Common;
using Maono.Application.Common.Interfaces;
using Maono.Application.Features.Auth.Commands;
using Maono.Application.Features.Auth.Queries;
using Maono.Domain.Identity.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

/// <summary>
/// Auth controller — delegates to MediatR CQRS pipeline.
/// Pipeline flow:
///   Request → LoggingBehavior → ValidationBehavior → TransactionBehavior (ICommand) → Handler
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Authentification")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    // ── 1. Self-Register (Freelancer / Agency) ─────────────────

    /// <summary>
    /// Inscription publique — crée un compte + workspace + rôle FreelancerOwner.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] SelfRegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(
            request.Email, request.Password,
            request.FirstName, request.LastName,
            request.PhoneNumber, request.WorkspaceName));

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Error!, 400));

        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Inscription réussie. Votre workspace a été créé."));
    }

    // ── 2. Register via Invitation ─────────────────────────────

    /// <summary>
    /// Inscription par invitation — rejoint un workspace existant avec un rôle prédéfini.
    /// </summary>
    [HttpPost("register/invite")]
    public async Task<IActionResult> RegisterByInvite([FromBody] InviteRegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterByInviteCommand(
            request.InviteToken, request.Password,
            request.FirstName, request.LastName, request.PhoneNumber));

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Error!, 400));

        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Inscription réussie. Vous avez rejoint le workspace."));
    }

    // ── 3. Admin Creates User ──────────────────────────────────

    /// <summary>
    /// Admin crée un utilisateur et l'assigne à un workspace avec un rôle.
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("register/admin")]
    public async Task<IActionResult> AdminCreateUser([FromBody] AdminCreateRequest request)
    {
        var result = await _mediator.Send(new AdminCreateUserCommand(
            request.Email, request.FirstName, request.LastName,
            request.PhoneNumber, request.Password,
            request.WorkspaceId, request.RoleName));

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Error!, 400));

        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Utilisateur créé et assigné au workspace."));
    }

    // ── Login ──────────────────────────────────────────────────

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var deviceType = DetectDeviceType(Request.Headers.UserAgent.ToString());

        var result = await _mediator.Send(new LoginCommand(
            request.Email,
            request.Password,
            request.DeviceName,
            deviceType,
            Request.Headers.UserAgent.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            request.DeviceFingerprint));

        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<object>.Error(result.Error!, 401, "auth_failed"));

        return Ok(ApiResponse<object>.Ok(result.Value!, "Connexion réussie"));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(
            request.AccessToken,
            request.RefreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString()));

        if (!result.IsSuccess)
            return Unauthorized(ApiResponse<object>.Error(result.Error!, 401, "token_invalid"));

        return Ok(ApiResponse<object>.Ok(result.Value!, "Token rafraîchi"));
    }

    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Error("Non authentifié.", 401, "unauthenticated"));

        var result = await _mediator.Send(new GetActiveSessionsQuery(userId.Value));

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Error!, 400));

        return Ok(ApiResponse<object>.Ok(result.Value!, "Sessions récupérées"));
    }

    [Authorize]
    [HttpPost("sessions/{sessionId}/revoke")]
    public async Task<IActionResult> RevokeSession(Guid sessionId)
    {
        var result = await _mediator.Send(new RevokeSessionCommand(sessionId));

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Error!, 400));

        return Ok(ApiResponse.Ok("Session révoquée — appareil déconnecté"));
    }

    [Authorize]
    [HttpPost("sessions/revoke-others")]
    public async Task<IActionResult> RevokeOtherSessions([FromBody] RevokeOtherSessionsRequest request)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized(ApiResponse<object>.Error("Non authentifié.", 401, "unauthenticated"));

        var result = await _mediator.Send(new RevokeAllOtherSessionsCommand(request.CurrentSessionId, userId.Value));

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Error!, 400));

        return Ok(ApiResponse.Ok("Toutes les autres sessions ont été révoquées"));
    }

    private static DeviceType DetectDeviceType(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return DeviceType.Unknown;
        var ua = userAgent.ToLowerInvariant();
        if (ua.Contains("mobile") || ua.Contains("android") || ua.Contains("iphone"))
            return DeviceType.Mobile;
        if (ua.Contains("tablet") || ua.Contains("ipad"))
            return DeviceType.Tablet;
        if (ua.Contains("postman") || ua.Contains("insomnia") || ua.Contains("curl"))
            return DeviceType.Api;
        return DeviceType.Laptop;
    }

    // ── My Workspaces ─────────────────────────────────────────

    /// <summary>
    /// Récupère tous les workspaces auxquels l'utilisateur connecté appartient,
    /// avec son rôle et le statut par défaut dans chacun.
    /// </summary>
    [Authorize]
    [HttpGet("me/workspaces")]
    public async Task<IActionResult> GetMyWorkspaces()
    {
        var result = await _mediator.Send(new Application.Features.Workspaces.Queries.GetMyWorkspacesQuery());
        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Error(result.Error!, 400));

        return Ok(ApiResponse<object>.Ok(result.Value!, "Workspaces récupérés"));
    }
}

// ── Request DTOs ────────────────────────────────────────────

/// <summary>Self-register: creates user + workspace</summary>
public record SelfRegisterRequest(
    string Email, string Password,
    string FirstName, string LastName,
    string? PhoneNumber, string WorkspaceName);

/// <summary>Register via invite token</summary>
public record InviteRegisterRequest(
    string InviteToken, string Password,
    string FirstName, string LastName,
    string? PhoneNumber);

/// <summary>Admin creates user in a specific workspace</summary>
public record AdminCreateRequest(
    string Email, string FirstName, string LastName,
    string? PhoneNumber, string? Password,
    Guid WorkspaceId, string RoleName);
public record LoginRequest(string Email, string Password, string? DeviceName = null, string? DeviceFingerprint = null);
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record RevokeOtherSessionsRequest(Guid CurrentSessionId);
