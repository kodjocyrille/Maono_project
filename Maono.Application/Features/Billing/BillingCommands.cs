using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Missions.Enums;

namespace Maono.Application.Features.Billing;

/// <summary>
/// ECR-023 — Billing & Delivery Note commands/queries.
/// </summary>
public record CreateBillingRecordCommand(
    Guid MissionId,
    decimal Amount,
    string? Currency,
    string? Notes
) : ICommand<Result<BillingRecordDto>>;

public record UpdateBillingStatusCommand(
    Guid BillingRecordId,
    BillingStatus NewStatus
) : ICommand<Result>;

public record GenerateDeliveryNoteCommand(
    Guid MissionDeliveryId,
    string? Reference
) : ICommand<Result<DeliveryNoteDto>>;

public record ListBillingRecordsQuery(Guid? MissionId) : IQuery<Result<List<BillingRecordDto>>>;

public record BillingRecordDto(
    Guid Id,
    Guid MissionId,
    decimal Amount,
    string Currency,
    string BillingStatus,
    DateTime? ExportedToOdooAtUtc,
    string? OdooInvoiceId,
    string? Notes,
    DateTime CreatedAtUtc
);

public record DeliveryNoteDto(
    Guid Id,
    Guid MissionDeliveryId,
    string? Reference,
    DateTime GeneratedAtUtc,
    string? StoragePath
);
