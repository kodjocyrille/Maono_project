using Maono.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Maono.Infrastructure.Odoo;

public class OdooBillingService : IOdooBillingService
{
    private readonly HttpClient _httpClient;
    private readonly OdooSettings _settings;
    private readonly ILogger<OdooBillingService> _logger;

    public OdooBillingService(HttpClient httpClient, IOptions<OdooSettings> settings, ILogger<OdooBillingService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string?> ExportInvoiceAsync(Guid billingRecordId, decimal amount, string currency, string? clientOdooId, CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting invoice for billing record {BillingRecordId} to Odoo. Amount: {Amount} {Currency}",
            billingRecordId, amount, currency);

        // TODO: Implement Odoo JSON-RPC or REST API call to create invoice
        // POST {BaseUrl}/api/v1/invoices or JSON-RPC to account.move
        // Return the Odoo invoice ID on success

        await Task.CompletedTask;
        _logger.LogWarning("Odoo billing export not yet implemented — returning null");
        return null;
    }

    public async Task<string?> GetInvoiceStatusAsync(string odooInvoiceId, CancellationToken ct = default)
    {
        _logger.LogInformation("Checking invoice status for Odoo ID {OdooInvoiceId}", odooInvoiceId);

        // TODO: Implement Odoo API call to check invoice status
        await Task.CompletedTask;
        return null;
    }
}
