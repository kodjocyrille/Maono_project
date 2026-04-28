using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Tasks.DTOs;
using Maono.Domain.Content.Entities;

namespace Maono.Application.Features.Tasks.Queries;

public record ListTasksQuery(Guid ContentItemId) : IQuery<Result<List<TaskDto>>>;

public record GetMyTasksQuery(ContentTaskStatus? Status) : IQuery<Result<List<TaskDto>>>;
