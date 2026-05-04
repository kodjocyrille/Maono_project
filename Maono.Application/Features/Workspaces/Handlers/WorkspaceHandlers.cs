using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Workspaces.Commands;
using Maono.Application.Features.Workspaces.DTOs;
using Maono.Application.Features.Workspaces.Queries;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Maono.Application.Features.Workspaces.Handlers;

public class CreateWorkspaceHandler : IRequestHandler<CreateWorkspaceCommand, Result<WorkspaceDto>>
{
    private readonly IWorkspaceRepository _repo;
    public CreateWorkspaceHandler(IWorkspaceRepository repo) => _repo = repo;

    public async Task<Result<WorkspaceDto>> Handle(CreateWorkspaceCommand request, CancellationToken ct)
    {
        var workspace = new Workspace
        {
            Name = request.Name,
            Slug = request.Slug,
            Plan = request.Plan,
            DefaultTimezone = request.DefaultTimezone,
            LogoUrl = request.LogoUrl
        };
        await _repo.AddAsync(workspace, ct);
        return Result.Success(new WorkspaceDto(
            workspace.Id, workspace.Name, workspace.Slug, workspace.Plan,
            workspace.DefaultTimezone, workspace.LogoUrl, workspace.CreatedAtUtc));
    }
}

public class GetWorkspaceByIdHandler : IRequestHandler<GetWorkspaceByIdQuery, Result<WorkspaceDetailDto>>
{
    private readonly IWorkspaceRepository _repo;

    public GetWorkspaceByIdHandler(IWorkspaceRepository repo) => _repo = repo;

    public async Task<Result<WorkspaceDetailDto>> Handle(GetWorkspaceByIdQuery request, CancellationToken ct)
    {
        var ws = await _repo.GetByIdAsync(request.Id, ct);
        if (ws == null) return Result.Failure<WorkspaceDetailDto>("Workspace not found", "NOT_FOUND");

        var members = ws.Memberships?.Select(m => new WorkspaceMemberDto(
            m.Id, m.UserId, m.User?.DisplayName ?? "", m.Role?.Name ?? "", m.JoinedAtUtc)).ToList()
            ?? new List<WorkspaceMemberDto>();

        return Result.Success(new WorkspaceDetailDto(
            ws.Id, ws.Name, ws.Slug, ws.Plan, ws.DefaultTimezone, ws.LogoUrl,
            ws.SettingsJson, ws.CreatedAtUtc, members));
    }
}

public class UpdateWorkspaceSettingsHandler : IRequestHandler<UpdateWorkspaceSettingsCommand, Result<WorkspaceDto>>
{
    private readonly IWorkspaceRepository _repo;

    public UpdateWorkspaceSettingsHandler(IWorkspaceRepository repo) => _repo = repo;

    public async Task<Result<WorkspaceDto>> Handle(UpdateWorkspaceSettingsCommand request, CancellationToken ct)
    {
        var ws = await _repo.GetByIdAsync(request.WorkspaceId, ct);
        if (ws == null) return Result.Failure<WorkspaceDto>("Workspace not found", "NOT_FOUND");

        if (request.SettingsJson != null) ws.SettingsJson = request.SettingsJson;
        if (request.DefaultTimezone != null) ws.DefaultTimezone = request.DefaultTimezone;
        if (request.LogoUrl != null) ws.LogoUrl = request.LogoUrl;
        _repo.Update(ws);

        return Result.Success(new WorkspaceDto(
            ws.Id, ws.Name, ws.Slug, ws.Plan, ws.DefaultTimezone, ws.LogoUrl, ws.CreatedAtUtc));
    }
}

public class ListWorkspacesHandler : IRequestHandler<ListWorkspacesQuery, Result<List<WorkspaceDto>>>
{
    private readonly IWorkspaceRepository _repo;
    public ListWorkspacesHandler(IWorkspaceRepository repo) => _repo = repo;

    public async Task<Result<List<WorkspaceDto>>> Handle(ListWorkspacesQuery request, CancellationToken ct)
    {
        var workspaces = await _repo.GetAllAsync(ct);
        var dtos = workspaces.Select(ws => new WorkspaceDto(
            ws.Id, ws.Name, ws.Slug, ws.Plan, ws.DefaultTimezone, ws.LogoUrl, ws.CreatedAtUtc)).ToList();
        return Result.Success(dtos);
    }
}

public class GetMyWorkspacesHandler : IRequestHandler<GetMyWorkspacesQuery, Result<List<MyWorkspaceDto>>>
{
    private readonly IMembershipRepository _membershipRepo;
    private readonly ICurrentUserService _currentUser;

    public GetMyWorkspacesHandler(IMembershipRepository membershipRepo, ICurrentUserService currentUser)
    {
        _membershipRepo = membershipRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<List<MyWorkspaceDto>>> Handle(GetMyWorkspacesQuery request, CancellationToken ct)
    {
        if (_currentUser.UserId is null)
            return Result.Failure<List<MyWorkspaceDto>>("Utilisateur non authentifié.", "UNAUTHORIZED");

        var memberships = await _membershipRepo.GetByUserIdAsync(_currentUser.UserId.Value, ct);

        var dtos = memberships.Select(m => new MyWorkspaceDto(
            m.WorkspaceId,
            m.Workspace?.Name ?? "",
            m.Workspace?.Slug ?? "",
            m.Workspace?.Plan,
            m.Workspace?.LogoUrl,
            m.Role?.Name ?? "",
            m.IsDefault,
            m.JoinedAtUtc
        )).ToList();

        return Result.Success(dtos);
    }
}

