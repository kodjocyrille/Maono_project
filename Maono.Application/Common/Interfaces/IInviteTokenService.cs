namespace Maono.Application.Common.Interfaces;

/// <summary>
/// Manages workspace invitation tokens.
/// </summary>
public interface IInviteTokenService
{
    /// <summary>
    /// Generate an invite token for a user to join a workspace with a specific role.
    /// </summary>
    Task<string> GenerateTokenAsync(Guid workspaceId, string workspaceName, string email, string roleName, CancellationToken ct = default);

    /// <summary>
    /// Validate an invite token. Returns null if invalid or expired.
    /// </summary>
    Task<InviteTokenPayload?> ValidateTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Mark a token as used after successful registration.
    /// </summary>
    Task MarkAsUsedAsync(string token, CancellationToken ct = default);
}

public record InviteTokenPayload(
    Guid WorkspaceId,
    string WorkspaceName,
    string Email,
    string RoleName
);
