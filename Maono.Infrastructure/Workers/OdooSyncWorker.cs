using Maono.Application.Common.Interfaces;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Campaigns.Enums;
using Maono.Domain.Content.Enums;
using Maono.Domain.Assets.Enums;
using Maono.Domain.Approval.Enums;
using Maono.Domain.Publications.Enums;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Notifications.Enums;
using Maono.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maono.Infrastructure.Workers;

/// <summary>
/// Synchronizes pending billing records and contacts to Odoo.
/// Runs every 30 minutes.
/// </summary>
public class OdooSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OdooSyncWorker> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

    public OdooSyncWorker(IServiceScopeFactory scopeFactory, ILogger<OdooSyncWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OdooSyncWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncBillingRecordsAsync(stoppingToken);
                await SyncClientsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OdooSyncWorker");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task SyncBillingRecordsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MaonoDbContext>();
        var billingService = scope.ServiceProvider.GetRequiredService<IOdooBillingService>();

        var pendingRecords = await context.BillingRecords
            .IgnoreQueryFilters()
            .Where(b => b.BillingStatus == BillingStatus.ToInvoice && b.ExportedToOdooAtUtc == null)
            .Include(b => b.Mission)
            .ToListAsync(ct);

        _logger.LogInformation("Syncing {Count} billing records to Odoo", pendingRecords.Count);

        foreach (var record in pendingRecords)
        {
            try
            {
                // Get client Odoo ID if available
                string? clientOdooId = null;
                if (record.Mission?.ClientOrganizationId != null)
                {
                    var client = await context.ClientOrganizations
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(c => c.Id == record.Mission.ClientOrganizationId, ct);
                    clientOdooId = client?.ExternalOdooId;
                }

                var odooInvoiceId = await billingService.ExportInvoiceAsync(
                    record.Id, record.Amount, record.Currency, clientOdooId, ct);

                if (odooInvoiceId != null)
                {
                    record.OdooInvoiceId = odooInvoiceId;
                    record.ExportedToOdooAtUtc = DateTime.UtcNow;
                    record.BillingStatus = BillingStatus.Invoiced;
                    _logger.LogInformation("Billing record {RecordId} exported to Odoo as {OdooId}",
                        record.Id, odooInvoiceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync billing record {RecordId} to Odoo", record.Id);

                // Record the failure
                context.IntegrationFailures.Add(new Domain.Audit.Entities.IntegrationFailure
                {
                    Provider = "Odoo",
                    CorrelationId = record.Id.ToString(),
                    ErrorCode = "BILLING_EXPORT_FAILED",
                    Details = ex.Message
                });
            }
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task SyncClientsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MaonoDbContext>();
        var contactSync = scope.ServiceProvider.GetRequiredService<IOdooContactSyncService>();

        // Sync clients that don't have an Odoo ID yet
        var unsyncedClients = await context.ClientOrganizations
            .IgnoreQueryFilters()
            .Where(c => c.ExternalOdooId == null && !c.IsDeleted)
            .Take(50) // batch
            .ToListAsync(ct);

        _logger.LogInformation("Syncing {Count} clients to Odoo CRM", unsyncedClients.Count);

        foreach (var client in unsyncedClients)
        {
            try
            {
                var odooId = await contactSync.SyncClientAsync(
                    client.Id, client.Name, client.BillingEmail, client.Phone, ct);

                if (odooId != null)
                {
                    client.ExternalOdooId = odooId;
                    _logger.LogInformation("Client {ClientId} synced to Odoo as {OdooId}", client.Id, odooId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync client {ClientId} to Odoo", client.Id);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}
