using Maono.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Maono.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core connection interceptor that sets the PostgreSQL session variable
/// for Row-Level Security tenant isolation.
/// </summary>
public class TenantConnectionInterceptor : Microsoft.EntityFrameworkCore.Diagnostics.DbConnectionInterceptor
{
    private readonly Func<Guid?> _getWorkspaceId;

    public TenantConnectionInterceptor(Func<Guid?> getWorkspaceId)
    {
        _getWorkspaceId = getWorkspaceId;
    }

    public override async Task ConnectionOpenedAsync(
        System.Data.Common.DbConnection connection,
        Microsoft.EntityFrameworkCore.Diagnostics.ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        var workspaceId = _getWorkspaceId();
        if (workspaceId.HasValue && connection is NpgsqlConnection npgsqlConnection)
        {
            await using var cmd = npgsqlConnection.CreateCommand();
            cmd.CommandText = $"SET app.current_workspace_id = '{workspaceId.Value}'";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
