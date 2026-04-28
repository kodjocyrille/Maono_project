namespace Maono.Application.Common.Interfaces;

/// <summary>
/// Odoo contact synchronization — push clients/contacts to Odoo CRM.
/// </summary>
public interface IOdooContactSyncService
{
    Task<string?> SyncClientAsync(Guid clientOrganizationId, string name, string? email, string? phone, CancellationToken ct = default);
    Task<string?> SyncContactAsync(string? clientOdooId, string fullName, string? email, string? phone, CancellationToken ct = default);
}
