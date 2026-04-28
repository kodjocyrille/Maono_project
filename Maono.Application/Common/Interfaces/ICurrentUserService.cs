namespace Maono.Application.Common.Interfaces;

/// <summary>
/// Resolves the current user and workspace context from the request.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? WorkspaceId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Permissions { get; }
    bool HasPermission(string permission);
}
