namespace Maono.Application.Common.Interfaces;

/// <summary>
/// Odoo billing integration — export invoices to Odoo.
/// </summary>
public interface IOdooBillingService
{
    Task<string?> ExportInvoiceAsync(Guid billingRecordId, decimal amount, string currency, string? clientOdooId, CancellationToken ct = default);
    Task<string?> GetInvoiceStatusAsync(string odooInvoiceId, CancellationToken ct = default);
}
