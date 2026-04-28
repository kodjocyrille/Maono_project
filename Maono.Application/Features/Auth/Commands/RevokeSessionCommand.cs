using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Auth.Commands;

/// <summary>
/// Revokes a specific device session (logout from one device).
/// </summary>
public record RevokeSessionCommand(Guid SessionId) : ICommand<Result>;

/// <summary>
/// Revokes all sessions except the current one (logout from all other devices).
/// </summary>
public record RevokeAllOtherSessionsCommand(Guid CurrentSessionId, Guid UserId) : ICommand<Result>;
