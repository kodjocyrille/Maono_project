using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.DTOs;

namespace Maono.Application.Features.Assets.Commands;

public record RestoreAssetVersionCommand(Guid AssetId, int TargetVersionNumber) : ICommand<Result<AssetDto>>;
