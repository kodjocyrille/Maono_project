using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;

namespace Maono.Application.Features.Assets.Commands;

public record InitiateUploadSessionCommand(
    Guid ContentItemId,
    string FileName,
    string MimeType,
    long DeclaredSizeBytes,
    string DeclaredSha256       // SHA-256 hex, 64 chars, computed client-side
) : ICommand<Result<UploadSessionDto>>;

public record ConfirmUploadSessionCommand(
    Guid SessionId,
    long ActualSizeBytes,
    string ActualSha256
) : ICommand<Result<AssetUploadConfirmedDto>>;

public record UploadSessionDto(
    Guid SessionId,
    string PresignedUrl,
    DateTime ExpiresAt
);

public record AssetUploadConfirmedDto(
    Guid AssetId,
    string StoragePath,
    int Version,
    string FileName
);
