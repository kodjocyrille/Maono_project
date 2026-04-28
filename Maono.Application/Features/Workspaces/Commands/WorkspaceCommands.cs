using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Workspaces.DTOs;

namespace Maono.Application.Features.Workspaces.Commands;

public record CreateWorkspaceCommand(
    string Name,
    string Slug,
    string? Plan,
    string? DefaultTimezone,
    string? LogoUrl
) : ICommand<Result<WorkspaceDto>>;

public record UpdateWorkspaceSettingsCommand(Guid WorkspaceId, string? SettingsJson, string? DefaultTimezone, string? LogoUrl)
    : ICommand<Result<WorkspaceDto>>;
