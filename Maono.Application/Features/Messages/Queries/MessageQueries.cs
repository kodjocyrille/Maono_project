using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Messages.DTOs;

namespace Maono.Application.Features.Messages.Queries;

public record GetContentMessagesQuery(Guid ContentItemId) : IQuery<Result<List<ContentMessageDto>>>;
