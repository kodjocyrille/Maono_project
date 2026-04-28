using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Messages.DTOs;

namespace Maono.Application.Features.Messages.Commands;

public record SendContentMessageCommand(Guid ContentItemId, string Body) : ICommand<Result<ContentMessageDto>>;
