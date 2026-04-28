using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Admin.Queries;

public record ListUsersQuery(
    string? Search = null,
    string? RoleFilter = null,
    bool ActiveOnly = true
) : IQuery<Result<List<UserAdminDto>>>;
