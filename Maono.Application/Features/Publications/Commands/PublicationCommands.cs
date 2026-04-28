using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Publications.DTOs;

namespace Maono.Application.Features.Publications.Commands;

public record SchedulePublicationCommand(Guid ContentItemId, Domain.Publications.Enums.SocialPlatform Platform, DateTime ScheduledAtUtc) : ICommand<Result<PublicationDto>>;
public record PublishNowCommand(Guid PublicationId) : ICommand<Result<PublicationDto>>;
public record RetryPublicationCommand(Guid PublicationId) : ICommand<Result<PublicationDto>>;
public record DeletePublicationCommand(Guid Id) : ICommand<Result>;
