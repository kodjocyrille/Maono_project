using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Admin.Commands;

public record CreateUserCommand(
    string Email,
    string DisplayName,
    string RoleName,
    string? Password
) : ICommand<Result<UserAdminDto>>;

public record UpdateUserCommand(
    Guid UserId,
    string? RoleName,
    bool? IsActive
) : ICommand<Result<UserAdminDto>>;

public record DeactivateUserCommand(Guid UserId) : ICommand<Result>;
