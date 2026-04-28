using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Content.Commands;

public record UpdateContentCommand(Guid Id, string Title, string? Format, int Priority, DateTime? Deadline) : ICommand<Result>;
public record DeleteContentCommand(Guid Id) : ICommand<Result>;
