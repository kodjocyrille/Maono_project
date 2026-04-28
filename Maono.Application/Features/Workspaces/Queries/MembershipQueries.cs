using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Workspaces.DTOs;

namespace Maono.Application.Features.Workspaces.Queries;

public record ListMembersQuery(Guid WorkspaceId) : IQuery<Result<List<MemberDto>>>;
