using System.Text.Json;
using Maono.Application.Common.Interfaces;
using Maono.Domain.Audit.Entities;
using Maono.Infrastructure.Persistence;

namespace Maono.Infrastructure.Services;

public class AuditWriter : IAuditWriter
{
    private readonly MaonoDbContext _context;

    public AuditWriter(MaonoDbContext context)
    {
        _context = context;
    }

    public async Task WriteAsync(Guid workspaceId, string action, string entityName, Guid? entityId,
        string? actorType, string? actorId, object? oldValue = null, object? newValue = null,
        CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            WorkspaceId = workspaceId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            ActorType = actorType,
            ActorId = actorId,
            OldValueJson = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
            NewValueJson = newValue != null ? JsonSerializer.Serialize(newValue) : null,
            OccurredAtUtc = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }
}
