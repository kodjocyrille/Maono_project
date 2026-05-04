using Maono.Application.Common.Models;
using Maono.Application.Features.Content.DTOs;
using Maono.Application.Features.Content.Queries;
using Maono.Domain.Content.Repository;
using MediatR;

namespace Maono.Application.Features.Content.Handlers;

public class GetContentByIdHandler : IRequestHandler<GetContentByIdQuery, Result<ContentItemDetailDto>>
{
    private readonly IContentRepository _repo;

    public GetContentByIdHandler(IContentRepository repo) => _repo = repo;

    public async Task<Result<ContentItemDetailDto>> Handle(GetContentByIdQuery request, CancellationToken ct)
    {
        var item = await _repo.GetWithDetailsAsync(request.Id, ct);
        if (item == null) return Result.Failure<ContentItemDetailDto>("Contenu introuvable.", "NOT_FOUND");

        var briefs = item.Briefs.Select(b => new BriefDto(b.Id, b.Body, b.CreatedAtUtc)).ToList();
        var checklist = item.ChecklistItems.Select(c => new ChecklistItemDto(c.Id, c.Label, c.IsCompleted)).ToList();

        return Result.Success(new ContentItemDetailDto(
            item.Id, item.Title, item.Format, item.Status,
            item.Deadline, item.Priority, item.CurrentRevisionNumber,
            item.CalendarEntryId, item.CalendarEntry?.CampaignId,
            item.CreatedAtUtc, briefs, checklist));
    }
}

public class ListContentHandler : IRequestHandler<ListContentQuery, Result<List<ContentItemDto>>>
{
    private readonly IContentRepository _repo;

    public ListContentHandler(IContentRepository repo) => _repo = repo;

    public async Task<Result<List<ContentItemDto>>> Handle(ListContentQuery request, CancellationToken ct)
    {
        var items = await _repo.GetAllAsync(ct);
        var dtos = items.Select(i => new ContentItemDto(
            i.Id, i.Title, i.Format, i.Status,
            i.Deadline, i.Priority, i.CurrentRevisionNumber,
            i.CalendarEntryId, i.CalendarEntry?.CampaignId,
            i.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}
