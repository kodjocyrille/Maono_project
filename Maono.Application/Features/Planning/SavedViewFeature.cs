using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Domain.Common;
using Maono.Domain.Planning.Entities;
using MediatR;

namespace Maono.Application.Features.Planning;

// ── ECR-034 — Saved Views CRUD ──────────────────────

public record CreateSavedViewCommand(string Name, string? FiltersJson) : ICommand<Result<SavedViewDto>>;
public record ListSavedViewsQuery() : IQuery<Result<List<SavedViewDto>>>;
public record DeleteSavedViewCommand(Guid Id) : ICommand<Result>;
public record SavedViewDto(Guid Id, Guid UserId, string Name, string? FiltersJson, DateTime CreatedAtUtc);

public class CreateSavedViewHandler : IRequestHandler<CreateSavedViewCommand, Result<SavedViewDto>>
{
    private readonly IGenericRepository<SavedView> _viewRepo;
    private readonly ICurrentUserService _currentUser;

    public CreateSavedViewHandler(IGenericRepository<SavedView> viewRepo, ICurrentUserService currentUser)
    {
        _viewRepo = viewRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<SavedViewDto>> Handle(CreateSavedViewCommand request, CancellationToken ct)
    {
        var view = new SavedView
        {
            WorkspaceId = _currentUser.WorkspaceId!.Value,
            UserId = _currentUser.UserId ?? Guid.Empty,
            Name = request.Name,
            FiltersJson = request.FiltersJson
        };
        await _viewRepo.AddAsync(view, ct);
        return Result.Success(new SavedViewDto(view.Id, view.UserId, view.Name, view.FiltersJson, view.CreatedAtUtc));
    }
}

public class ListSavedViewsHandler : IRequestHandler<ListSavedViewsQuery, Result<List<SavedViewDto>>>
{
    private readonly IGenericRepository<SavedView> _viewRepo;
    private readonly ICurrentUserService _currentUser;

    public ListSavedViewsHandler(IGenericRepository<SavedView> viewRepo, ICurrentUserService currentUser)
    {
        _viewRepo = viewRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<SavedViewDto>>> Handle(ListSavedViewsQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? Guid.Empty;
        var views = await _viewRepo.FindAsync(v => v.UserId == userId, ct);
        var dtos = views.Select(v => new SavedViewDto(v.Id, v.UserId, v.Name, v.FiltersJson, v.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}

public class DeleteSavedViewHandler : IRequestHandler<DeleteSavedViewCommand, Result>
{
    private readonly IGenericRepository<SavedView> _viewRepo;
    public DeleteSavedViewHandler(IGenericRepository<SavedView> viewRepo) => _viewRepo = viewRepo;

    public async Task<Result> Handle(DeleteSavedViewCommand request, CancellationToken ct)
    {
        var view = await _viewRepo.GetByIdAsync(request.Id, ct);
        if (view == null) return Result.Failure("Vue sauvegardée introuvable.", "NOT_FOUND");
        _viewRepo.Remove(view);
        return Result.Success();
    }
}
