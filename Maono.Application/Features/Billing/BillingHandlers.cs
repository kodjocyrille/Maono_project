using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Common;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Missions.Enums;
using Maono.Domain.Missions.Repository;
using MediatR;

namespace Maono.Application.Features.Billing;

/// <summary>
/// ECR-023 — Create a billing record for a mission.
/// </summary>
public class CreateBillingRecordHandler : IRequestHandler<CreateBillingRecordCommand, Result<BillingRecordDto>>
{
    private readonly IMissionRepository _missionRepo;
    private readonly IGenericRepository<BillingRecord> _billingRepo;
    private readonly ICurrentUserService _currentUser;

    public CreateBillingRecordHandler(IMissionRepository missionRepo, IGenericRepository<BillingRecord> billingRepo, ICurrentUserService currentUser)
    {
        _missionRepo = missionRepo;
        _billingRepo = billingRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<BillingRecordDto>> Handle(CreateBillingRecordCommand request, CancellationToken ct)
    {
        var mission = await _missionRepo.GetByIdAsync(request.MissionId, ct);
        if (mission == null) return Result.Failure<BillingRecordDto>("Mission introuvable.", "NOT_FOUND");

        if (request.Amount <= 0)
            return Result.Failure<BillingRecordDto>("Le montant doit être positif.", "INVALID_AMOUNT");

        var record = new BillingRecord
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            MissionId = request.MissionId,
            Amount = request.Amount,
            Currency = request.Currency ?? "EUR",
            Notes = request.Notes,
            BillingStatus = BillingStatus.Draft
        };
        await _billingRepo.AddAsync(record, ct);

        return Result.Success(MapToDto(record));
    }

    private static BillingRecordDto MapToDto(BillingRecord r) => new(
        r.Id, r.MissionId, r.Amount, r.Currency, r.BillingStatus.ToString(),
        r.ExportedToOdooAtUtc, r.OdooInvoiceId, r.Notes, r.CreatedAtUtc);
}

/// <summary>
/// ECR-023 — Update billing status (Draft → Sent → Paid).
/// </summary>
public class UpdateBillingStatusHandler : IRequestHandler<UpdateBillingStatusCommand, Result>
{
    private readonly IGenericRepository<BillingRecord> _billingRepo;
    public UpdateBillingStatusHandler(IGenericRepository<BillingRecord> billingRepo) => _billingRepo = billingRepo;

    public async Task<Result> Handle(UpdateBillingStatusCommand request, CancellationToken ct)
    {
        var record = await _billingRepo.GetByIdAsync(request.BillingRecordId, ct);
        if (record == null) return Result.Failure("Enregistrement de facturation introuvable.", "NOT_FOUND");

        record.BillingStatus = request.NewStatus;
        return Result.Success();
    }
}

/// <summary>
/// ECR-023 — List billing records, optionally filtered by mission.
/// </summary>
public class ListBillingRecordsHandler : IRequestHandler<ListBillingRecordsQuery, Result<List<BillingRecordDto>>>
{
    private readonly IGenericRepository<BillingRecord> _billingRepo;
    public ListBillingRecordsHandler(IGenericRepository<BillingRecord> billingRepo) => _billingRepo = billingRepo;

    public async Task<Result<List<BillingRecordDto>>> Handle(ListBillingRecordsQuery request, CancellationToken ct)
    {
        var records = request.MissionId.HasValue
            ? await _billingRepo.FindAsync(r => r.MissionId == request.MissionId.Value, ct)
            : await _billingRepo.FindAsync(_ => true, ct);

        var dtos = records.Select(r => new BillingRecordDto(
            r.Id, r.MissionId, r.Amount, r.Currency, r.BillingStatus.ToString(),
            r.ExportedToOdooAtUtc, r.OdooInvoiceId, r.Notes, r.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}

/// <summary>
/// ECR-023 — Generate a delivery note for a mission delivery.
/// </summary>
public class GenerateDeliveryNoteHandler : IRequestHandler<GenerateDeliveryNoteCommand, Result<DeliveryNoteDto>>
{
    private readonly IGenericRepository<DeliveryNote> _noteRepo;
    private readonly ICurrentUserService _currentUser;

    public GenerateDeliveryNoteHandler(IGenericRepository<DeliveryNote> noteRepo, ICurrentUserService currentUser)
    {
        _noteRepo = noteRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<DeliveryNoteDto>> Handle(GenerateDeliveryNoteCommand request, CancellationToken ct)
    {
        var note = new DeliveryNote
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            MissionDeliveryId = request.MissionDeliveryId,
            Reference = request.Reference ?? $"BDL-{DateTime.UtcNow:yyyyMMdd-HHmmss}",
            GeneratedAtUtc = DateTime.UtcNow
        };
        await _noteRepo.AddAsync(note, ct);

        return Result.Success(new DeliveryNoteDto(
            note.Id, note.MissionDeliveryId, note.Reference, note.GeneratedAtUtc, note.StoragePath));
    }
}
