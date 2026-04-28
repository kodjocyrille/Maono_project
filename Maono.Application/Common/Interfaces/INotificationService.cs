namespace Maono.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAsync(Guid workspaceId, Guid userId, string type, string subject, string? body = null, CancellationToken ct = default);
    Task SendToWorkspaceAsync(Guid workspaceId, string type, string subject, string? body = null, CancellationToken ct = default);
}
