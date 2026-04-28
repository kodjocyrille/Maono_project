using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.Commands;

namespace Maono.Application.Features.Assets.Queries;

public record GetUploadSessionQuery(Guid SessionId) : IQuery<Result<UploadSessionDto>>;
