using Maono.Application.Common.Models;
using Maono.Application.Features.Auth.Queries;
using Maono.Domain.Identity.Entities;

namespace Maono.Application.Common.Interfaces;

/// <summary>
/// Authentication service abstraction. Used by Auth CQRS handlers.
/// Implementations live in Infrastructure and handle Identity + Domain user management.
/// </summary>
public interface IAuthenticationService
{
    // Registration
    Task<Result<User>> RegisterAsync(string email, string password, string displayName, CancellationToken ct = default);
    Task<Result<User>> RegisterAsync(string email, string password, string firstName, string lastName, string? phoneNumber, string? displayName = null, CancellationToken ct = default);

    // Authentication
    Task<Result<User>> AuthenticateAsync(string email, string password, CancellationToken ct = default);
    Task<List<string>> GetUserPermissionsAsync(Guid userId, Guid? roleId, CancellationToken ct = default);

    // User
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetUserWithMembershipsAsync(Guid userId, CancellationToken ct = default);
    Task UpdateLastLoginAsync(Guid userId, CancellationToken ct = default);

    // Sessions
    Task CreateSessionAsync(DeviceSession session, CancellationToken ct = default);
    Task<List<SessionDto>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default);
    Task RevokeSessionAsync(Guid sessionId, CancellationToken ct = default);
    Task RevokeAllSessionsExceptAsync(Guid userId, Guid keepSessionId, CancellationToken ct = default);
    Task UpdateSessionLastActiveAsync(Guid sessionId, string? ipAddress, CancellationToken ct = default);

    // Refresh tokens
    Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(RefreshToken token, string reason, CancellationToken ct = default);
}
