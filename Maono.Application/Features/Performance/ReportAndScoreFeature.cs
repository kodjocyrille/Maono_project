using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Common;
using Maono.Domain.Performance.Entities;
using MediatR;

namespace Maono.Application.Features.Performance;

// ── ECR-027 — Report Export (CSV/PDF metadata) ────────────

public record CreateReportExportCommand(
    Guid? CampaignId,
    Guid? ClientOrganizationId,
    string Format,
    DateTime? PeriodStart,
    DateTime? PeriodEnd
) : ICommand<Result<ReportExportDto>>;

public record ListReportExportsQuery() : IQuery<Result<List<ReportExportDto>>>;

public record ReportExportDto(
    Guid Id,
    Guid? CampaignId,
    Guid? ClientOrganizationId,
    string Format,
    string? StoragePath,
    DateTime? PeriodStart,
    DateTime? PeriodEnd,
    DateTime GeneratedAtUtc,
    string? GeneratedBy
);

// ── ECR-028/029 — Freelance Performance Score ────────────

public record GetFreelanceScoreQuery(Guid UserId) : IQuery<Result<FreelanceScoreDto>>;

public record FreelanceScoreDto(
    Guid UserId,
    int TotalTasksCompleted,
    int TotalTasksLate,
    decimal OnTimeRate,
    decimal? AverageRating,
    string PerformanceTier
);

// ── Handlers ────────────────────────────────────────────

public class CreateReportExportHandler : IRequestHandler<CreateReportExportCommand, Result<ReportExportDto>>
{
    private readonly IGenericRepository<ReportExport> _exportRepo;
    private readonly ICurrentUserService _currentUser;

    public CreateReportExportHandler(IGenericRepository<ReportExport> exportRepo, ICurrentUserService currentUser)
    {
        _exportRepo = exportRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<ReportExportDto>> Handle(CreateReportExportCommand request, CancellationToken ct)
    {
        var export = new ReportExport
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            CampaignId = request.CampaignId,
            ClientOrganizationId = request.ClientOrganizationId,
            Format = request.Format,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            GeneratedAtUtc = DateTime.UtcNow,
            GeneratedBy = _currentUser.UserId?.ToString(),
            StoragePath = $"reports/{DateTime.UtcNow:yyyyMMdd}/{Guid.NewGuid()}.{request.Format.ToLower()}"
        };
        await _exportRepo.AddAsync(export, ct);

        return Result.Success(MapToDto(export));
    }

    private static ReportExportDto MapToDto(ReportExport e) => new(
        e.Id, e.CampaignId, e.ClientOrganizationId, e.Format, e.StoragePath,
        e.PeriodStart, e.PeriodEnd, e.GeneratedAtUtc, e.GeneratedBy);
}

public class ListReportExportsHandler : IRequestHandler<ListReportExportsQuery, Result<List<ReportExportDto>>>
{
    private readonly IGenericRepository<ReportExport> _exportRepo;
    public ListReportExportsHandler(IGenericRepository<ReportExport> exportRepo) => _exportRepo = exportRepo;

    public async Task<Result<List<ReportExportDto>>> Handle(ListReportExportsQuery request, CancellationToken ct)
    {
        var exports = await _exportRepo.FindAsync(_ => true, ct);
        return Result.Success(exports.Select(e => new ReportExportDto(
            e.Id, e.CampaignId, e.ClientOrganizationId, e.Format, e.StoragePath,
            e.PeriodStart, e.PeriodEnd, e.GeneratedAtUtc, e.GeneratedBy)).ToList());
    }
}

/// <summary>
/// ECR-028/029 — Freelance performance score computed from tasks.
/// </summary>
public class GetFreelanceScoreHandler : IRequestHandler<GetFreelanceScoreQuery, Result<FreelanceScoreDto>>
{
    private readonly IGenericRepository<Domain.Content.Entities.ContentTask> _taskRepo;

    public GetFreelanceScoreHandler(IGenericRepository<Domain.Content.Entities.ContentTask> taskRepo) =>
        _taskRepo = taskRepo;

    public async Task<Result<FreelanceScoreDto>> Handle(GetFreelanceScoreQuery request, CancellationToken ct)
    {
        var tasks = await _taskRepo.FindAsync(t => t.AssignedToUserId == request.UserId, ct);
        var completed = tasks.Where(t => t.Status == Domain.Content.Entities.ContentTaskStatus.Completed).ToList();
        var total = completed.Count;
        var late = completed.Count(t => t.DueDate.HasValue && t.CompletedAtUtc.HasValue && t.CompletedAtUtc > t.DueDate);
        var onTime = total > 0 ? Math.Round((decimal)(total - late) / total * 100, 1) : 100m;

        var tier = onTime switch
        {
            >= 95 => "Excellent",
            >= 85 => "Bon",
            >= 70 => "Moyen",
            _ => "À améliorer"
        };

        return Result.Success(new FreelanceScoreDto(
            request.UserId, total, late, onTime, null, tier));
    }
}
