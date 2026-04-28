using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Workspaces.DTOs;

namespace Maono.Application.Features.Workspaces.Queries;

public record GetWorkspaceByIdQuery(Guid Id) : IQuery<Result<WorkspaceDetailDto>>;
public record ListWorkspacesQuery() : IQuery<Result<List<WorkspaceDto>>>;
