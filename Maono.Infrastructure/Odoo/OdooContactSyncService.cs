using Maono.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maono.Infrastructure.Odoo;

public class OdooContactSyncService : IOdooContactSyncService
{
    private readonly HttpClient _httpClient;
    private readonly OdooSettings _settings;
    private readonly ILogger<OdooContactSyncService> _logger;

    public OdooContactSyncService(HttpClient httpClient, IOptions<OdooSettings> settings, ILogger<OdooContactSyncService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string?> SyncClientAsync(Guid clientOrganizationId, string name, string? email, string? phone, CancellationToken ct = default)
    {
        _logger.LogInformation("Syncing client {ClientId} ({Name}) to Odoo CRM", clientOrganizationId, name);

        // TODO: Implement Odoo JSON-RPC call to res.partner (company)
        await Task.CompletedTask;
        _logger.LogWarning("Odoo contact sync not yet implemented — returning null");
        return null;
    }

    public async Task<string?> SyncContactAsync(string? clientOdooId, string fullName, string? email, string? phone, CancellationToken ct = default)
    {
        _logger.LogInformation("Syncing contact {Name} under Odoo partner {PartnerId}", fullName, clientOdooId);

        // TODO: Implement Odoo JSON-RPC call to res.partner (contact)
        await Task.CompletedTask;
        return null;
    }
}
