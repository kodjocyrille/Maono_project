namespace Maono.Application.Features.Messages.DTOs;

public record ContentMessageDto(Guid Id, Guid ContentItemId, string? AuthorType, Guid? AuthorId, string Body, DateTime SentAtUtc);
