using Maono.Application.Common.Models;
using Maono.Application.Features.Planning.Commands;
using Maono.Application.Features.Planning.DTOs;
using Maono.Domain.Planning.Entities;
using Maono.Domain.Planning.Repository;
using Maono.Domain.Publications.Enums;
using MediatR;

namespace Maono.Application.Features.Planning.Handlers;

public class CreateCalendarEntryHandler : IRequestHandler<CreateCalendarEntryCommand, Result<CalendarEntryDto>>
{
    private readonly ICalendarRepository _repo;
    public CreateCalendarEntryHandler(ICalendarRepository repo) => _repo = repo;

    public async Task<Result<CalendarEntryDto>> Handle(CreateCalendarEntryCommand request, CancellationToken ct)
    {
        if (request.PublicationDate.Date < DateTime.UtcNow.Date)
            return Result.Failure<CalendarEntryDto>("La date de publication ne peut pas être dans le passé.", "INVALID_DATE");

        if (!Enum.TryParse<SocialPlatform>(request.Platform, true, out var platform))
            return Result.Failure<CalendarEntryDto>($"Plateforme invalide : {request.Platform}", "INVALID_PLATFORM");

        var entry = new CalendarEntry
        {
            CampaignId = request.CampaignId,
            PublicationDate = request.PublicationDate,
            Platform = platform,
            ContentType = request.ContentType,
            Theme = request.Theme,
            Status = "Draft"
        };
        await _repo.AddAsync(entry, ct);
        return Result.Success(new CalendarEntryDto(
            entry.Id, entry.CampaignId, entry.PublicationDate,
            entry.Platform.ToString(), entry.ContentType, entry.Theme,
            entry.Status, entry.CreatedAtUtc));
    }
}

public class UpdateCalendarEntryHandler : IRequestHandler<UpdateCalendarEntryCommand, Result<CalendarEntryDto>>
{
    private readonly ICalendarRepository _repo;
    public UpdateCalendarEntryHandler(ICalendarRepository repo) => _repo = repo;

    public async Task<Result<CalendarEntryDto>> Handle(UpdateCalendarEntryCommand request, CancellationToken ct)
    {
        var entry = await _repo.GetByIdAsync(request.Id, ct);
        if (entry == null) return Result.Failure<CalendarEntryDto>("Entrée introuvable.", "NOT_FOUND");

        if (entry.Status is "Validated" or "Published")
            return Result.Failure<CalendarEntryDto>("Une entrée validée ou publiée ne peut pas être modifiée.", "IMMUTABLE");

        if (request.PublicationDate.HasValue) entry.PublicationDate = request.PublicationDate.Value;
        if (request.ContentType != null) entry.ContentType = request.ContentType;
        if (request.Theme != null) entry.Theme = request.Theme;
        if (request.Platform != null && Enum.TryParse<SocialPlatform>(request.Platform, true, out var p))
            entry.Platform = p;

        _repo.Update(entry);
        return Result.Success(new CalendarEntryDto(
            entry.Id, entry.CampaignId, entry.PublicationDate,
            entry.Platform.ToString(), entry.ContentType, entry.Theme,
            entry.Status, entry.CreatedAtUtc));
    }
}

public class DeleteCalendarEntryHandler : IRequestHandler<DeleteCalendarEntryCommand, Result>
{
    private readonly ICalendarRepository _repo;
    public DeleteCalendarEntryHandler(ICalendarRepository repo) => _repo = repo;

    public async Task<Result> Handle(DeleteCalendarEntryCommand request, CancellationToken ct)
    {
        var entry = await _repo.GetByIdAsync(request.Id, ct);
        if (entry == null) return Result.Failure("Entrée introuvable.", "NOT_FOUND");
        _repo.Remove(entry);
        return Result.Success();
    }
}

public class ValidateCalendarEntryHandler : IRequestHandler<ValidateCalendarEntryCommand, Result<CalendarEntryDto>>
{
    private readonly ICalendarRepository _repo;
    public ValidateCalendarEntryHandler(ICalendarRepository repo) => _repo = repo;

    public async Task<Result<CalendarEntryDto>> Handle(ValidateCalendarEntryCommand request, CancellationToken ct)
    {
        var entry = await _repo.GetByIdAsync(request.Id, ct);
        if (entry == null) return Result.Failure<CalendarEntryDto>("Entrée introuvable.", "NOT_FOUND");
        entry.Status = "Validated";
        _repo.Update(entry);
        return Result.Success(new CalendarEntryDto(
            entry.Id, entry.CampaignId, entry.PublicationDate,
            entry.Platform.ToString(), entry.ContentType, entry.Theme,
            entry.Status, entry.CreatedAtUtc));
    }
}
