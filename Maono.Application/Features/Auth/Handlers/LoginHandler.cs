using Maono.Domain.Common;
using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Auth.Commands;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Content.Enums;
using Maono.Domain.Assets.Enums;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Notifications.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Maono.Application.Features.Auth.Handlers;

/// <summary>
/// Handles LoginCommand: authenticates, creates DeviceSession, issues tokens.
/// The TransactionBehavior wraps this in a DB transaction automatically.
/// The ValidationBehavior validates the command before reaching here.
/// Pipeline: Request → ValidationBehavior → TransactionBehavior → LoginHandler
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(IAuthenticationService authService, IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Authenticate via Identity
        var authResult = await _authService.AuthenticateAsync(request.Email, request.Password, cancellationToken);
        if (!authResult.IsSuccess)
            return Result.Failure<LoginResponse>(authResult.Error!, "AUTH_FAILED");

        var user = authResult.Value!;

        // 2. Resolve default workspace + permissions
        var defaultMembership = user.Memberships.FirstOrDefault(m => m.IsDefault)
            ?? user.Memberships.FirstOrDefault();

        var permissions = await _authService.GetUserPermissionsAsync(user.Id, defaultMembership?.RoleId, cancellationToken);
        var roleName = defaultMembership?.Role?.Name;

        // 3. Create device session
        var session = new DeviceSession
        {
            UserId = user.Id,
            DeviceType = request.DeviceType,
            DeviceName = request.DeviceName,
            UserAgent = request.UserAgent,
            IpAddress = request.IpAddress,
            DeviceFingerprint = request.DeviceFingerprint
        };
        await _authService.CreateSessionAsync(session, cancellationToken);

        // 4. Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.Id.ToString(), user.Email, user.DisplayName,
            defaultMembership?.WorkspaceId, roleName, permissions);

        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            DeviceSessionId = session.Id,
            Token = refreshTokenValue,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };
        await _authService.SaveRefreshTokenAsync(refreshToken, cancellationToken);

        // 5. Update last login
        await _authService.UpdateLastLoginAsync(user.Id, cancellationToken);

        // CommitAsync is called by TransactionBehavior after this returns

        return Result.Success(new LoginResponse(
            accessToken, refreshTokenValue,
            user.Id, user.Email, user.DisplayName,
            defaultMembership?.WorkspaceId,
            session.Id));
    }
}
