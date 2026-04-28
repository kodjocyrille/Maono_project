using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Auth.Commands;
using Maono.Domain.Identity.Entities;
using MediatR;

namespace Maono.Application.Features.Auth.Handlers;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenHandler(IAuthenticationService authService, IJwtTokenService jwtTokenService)
    {
        _authService = authService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate expired access token to get claims
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return Result.Failure<RefreshTokenResponse>("Jeton d'accès invalide.", "INVALID_TOKEN");

        // 2. Find and validate refresh token
        var storedToken = await _authService.GetActiveRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (storedToken == null)
            return Result.Failure<RefreshTokenResponse>("Jeton de rafraîchissement invalide ou expiré.", "INVALID_REFRESH_TOKEN");

        // 3. Revoke old token (rotation)
        await _authService.RevokeRefreshTokenAsync(storedToken, "Remplacé par un nouveau jeton", cancellationToken);

        // 4. Get user with memberships for new access token
        var user = await _authService.GetUserWithMembershipsAsync(storedToken.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<RefreshTokenResponse>("Utilisateur introuvable.", "USER_NOT_FOUND");

        var defaultMembership = user.Memberships.FirstOrDefault(m => m.IsDefault)
            ?? user.Memberships.FirstOrDefault();
        var roleName = defaultMembership?.Role?.Name;
        var permissions = await _authService.GetUserPermissionsAsync(user.Id, defaultMembership?.RoleId, cancellationToken);

        // 5. Issue new token pair (same session)
        var newAccessToken = _jwtTokenService.GenerateAccessToken(
            user.Id.ToString(), user.Email, user.DisplayName,
            defaultMembership?.WorkspaceId, roleName, permissions);

        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            DeviceSessionId = storedToken.DeviceSessionId,
            Token = newRefreshTokenValue,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };
        storedToken.ReplacedByToken = newRefreshTokenValue;
        await _authService.SaveRefreshTokenAsync(newRefreshToken, cancellationToken);

        // 6. Update session last active
        await _authService.UpdateSessionLastActiveAsync(storedToken.DeviceSessionId, request.IpAddress, cancellationToken);

        return Result.Success(new RefreshTokenResponse(
            newAccessToken, newRefreshTokenValue, storedToken.DeviceSessionId));
    }
}
