namespace Maono.Application.Common.Interfaces;

public interface IAuditWriter
{
    Task WriteAsync(Guid workspaceId, string action, string entityName, Guid? entityId,
        string? actorType, string? actorId, object? oldValue = null, object? newValue = null,
        CancellationToken ct = default);
}
