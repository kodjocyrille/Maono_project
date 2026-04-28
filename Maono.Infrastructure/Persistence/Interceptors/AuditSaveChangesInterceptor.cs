using System.Text.Json;
using Maono.Application.Common.Interfaces;
using Maono.Domain.Audit.Entities;
using Maono.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Maono.Infrastructure.Persistence.Interceptors;

/// <summary>
/// ECR-018 — Automatically writes AuditLog entries for tracked entity changes.
/// Captures Create, Update, and Delete actions with old/new JSON snapshots.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private List<AuditEntry> _pendingAuditEntries = new();

    public AuditSaveChangesInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        OnBeforeSave(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        OnBeforeSave(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        OnAfterSave(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        await OnAfterSaveAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void OnBeforeSave(DbContext? context)
    {
        if (context == null) return;
        _pendingAuditEntries.Clear();

        var trackedEntities = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is not AuditLog) // don't audit the audit log
            .ToList();

        foreach (var entry in trackedEntities)
        {
            _pendingAuditEntries.Add(CreateAuditEntry(entry));
        }
    }

    private void OnAfterSave(DbContext? context)
    {
        if (context == null || _pendingAuditEntries.Count == 0) return;

        foreach (var audit in _pendingAuditEntries)
        {
            // For Added entities, capture the generated ID after save
            if (audit.Entry.State == EntityState.Added && audit.Entry.Entity is BaseEntity be)
            {
                audit.Log.EntityId = be.Id;
                audit.Log.NewValueJson = SerializeProperties(audit.Entry, EntityState.Added);
            }
            context.Set<AuditLog>().Add(audit.Log);
        }
        context.SaveChanges();
        _pendingAuditEntries.Clear();
    }

    private async Task OnAfterSaveAsync(DbContext? context, CancellationToken ct)
    {
        if (context == null || _pendingAuditEntries.Count == 0) return;

        foreach (var audit in _pendingAuditEntries)
        {
            if (audit.Entry.State == EntityState.Added && audit.Entry.Entity is BaseEntity be)
            {
                audit.Log.EntityId = be.Id;
                audit.Log.NewValueJson = SerializeProperties(audit.Entry, EntityState.Added);
            }
            context.Set<AuditLog>().Add(audit.Log);
        }
        await context.SaveChangesAsync(ct);
        _pendingAuditEntries.Clear();
    }

    private AuditEntry CreateAuditEntry(EntityEntry entry)
    {
        var entityName = entry.Entity.GetType().Name;
        var entityId = entry.Entity is BaseEntity baseEntity ? baseEntity.Id : (Guid?)null;
        var workspaceId = entry.Entity is TenantEntity tenantEntity ? tenantEntity.WorkspaceId : (Guid?)null;

        var log = new AuditLog
        {
            Action = entry.State.ToString(),
            EntityName = entityName,
            EntityId = entityId,
            ActorType = "User",
            ActorId = _currentUser.UserId?.ToString(),
            OccurredAtUtc = DateTime.UtcNow,
        };

        if (workspaceId.HasValue)
            log.WorkspaceId = workspaceId.Value;

        switch (entry.State)
        {
            case EntityState.Added:
                // NewValueJson will be set after save (when IDs are generated)
                break;
            case EntityState.Modified:
                log.OldValueJson = SerializeModifiedOriginal(entry);
                log.NewValueJson = SerializeModifiedCurrent(entry);
                break;
            case EntityState.Deleted:
                log.OldValueJson = SerializeProperties(entry, EntityState.Deleted);
                break;
        }

        return new AuditEntry(entry, log);
    }

    private static string? SerializeProperties(EntityEntry entry, EntityState state)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties.Where(p => !p.Metadata.IsShadowProperty()))
        {
            dict[prop.Metadata.Name] = state == EntityState.Deleted ? prop.OriginalValue : prop.CurrentValue;
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict) : null;
    }

    private static string? SerializeModifiedOriginal(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties.Where(p => p.IsModified && !p.Metadata.IsShadowProperty()))
        {
            dict[prop.Metadata.Name] = prop.OriginalValue;
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict) : null;
    }

    private static string? SerializeModifiedCurrent(EntityEntry entry)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties.Where(p => p.IsModified && !p.Metadata.IsShadowProperty()))
        {
            dict[prop.Metadata.Name] = prop.CurrentValue;
        }
        return dict.Count > 0 ? JsonSerializer.Serialize(dict) : null;
    }

    private record AuditEntry(EntityEntry Entry, AuditLog Log);
}
