using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Maono.Application.Common.Interfaces;
using Maono.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Maono.Infrastructure.Invitations;

/// <summary>
/// Invite tokens are JWTs with short TTL (7 days) containing workspace + role info.
/// No database storage needed — the token itself carries the payload.
/// </summary>
public class JwtInviteTokenService : IInviteTokenService
{
    private readonly JwtSettings _jwtSettings;
    private const int InviteTokenExpiryDays = 7;

    public JwtInviteTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public Task<string> GenerateTokenAsync(
        Guid workspaceId, string workspaceName, string email, string roleName, CancellationToken ct = default)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("invite_type", "workspace_join"),
            new Claim("workspace_id", workspaceId.ToString()),
            new Claim("workspace_name", workspaceName),
            new Claim("email", email),
            new Claim("role", roleName),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: "MaonoInvite",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(InviteTokenExpiryDays),
            signingCredentials: credentials);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public Task<InviteTokenPayload?> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var handler = new JwtSecurityTokenHandler();

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = "MaonoInvite",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            }, out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;

            var inviteType = jwt.Claims.FirstOrDefault(c => c.Type == "invite_type")?.Value;
            if (inviteType != "workspace_join")
                return Task.FromResult<InviteTokenPayload?>(null);

            var payload = new InviteTokenPayload(
                WorkspaceId: Guid.Parse(jwt.Claims.First(c => c.Type == "workspace_id").Value),
                WorkspaceName: jwt.Claims.First(c => c.Type == "workspace_name").Value,
                Email: jwt.Claims.First(c => c.Type == "email").Value,
                RoleName: jwt.Claims.First(c => c.Type == "role").Value
            );

            return Task.FromResult<InviteTokenPayload?>(payload);
        }
        catch
        {
            return Task.FromResult<InviteTokenPayload?>(null);
        }
    }

    public Task MarkAsUsedAsync(string token, CancellationToken ct = default)
    {
        // JWT-based tokens are stateless — no DB update needed.
        // If single-use is required, store used tokens in a blacklist table.
        return Task.CompletedTask;
    }
}
