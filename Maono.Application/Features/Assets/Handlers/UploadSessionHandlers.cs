using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Assets.Commands;
using Maono.Application.Features.Assets.Queries;
using Maono.Domain.Assets.Entities;
using Maono.Domain.Assets.Repository;
using MediatR;

namespace Maono.Application.Features.Assets.Handlers;

public class InitiateUploadSessionHandler : IRequestHandler<InitiateUploadSessionCommand, Result<UploadSessionDto>>
{
    private readonly IAssetStorageService _storage;
    private readonly IAssetUploadSessionRepository _sessionRepo;
    private readonly ICurrentUserService _currentUser;

    public InitiateUploadSessionHandler(
        IAssetStorageService storage,
        IAssetUploadSessionRepository sessionRepo,
        ICurrentUserService currentUser)
    {
        _storage = storage;
        _sessionRepo = sessionRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<UploadSessionDto>> Handle(InitiateUploadSessionCommand request, CancellationToken ct)
    {
        var sessionId = Guid.NewGuid();
        var workspaceId = _currentUser.WorkspaceId!.Value;
        var userId = _currentUser.UserId!.Value;

        // Build isolated storage key
        var storageKey = $"{workspaceId}/{request.ContentItemId}/{sessionId}/{request.FileName}";
        var ttl = TimeSpan.FromMinutes(15);

        var presigned = await _storage.GeneratePresignedPutUrlAsync(storageKey, request.MimeType, ttl, ct);

        var session = new AssetUploadSession
        {
            Id = sessionId,
            WorkspaceId = workspaceId,
            ContentItemId = request.ContentItemId,
            InitiatedByUserId = userId,
            FileName = request.FileName,
            MimeType = request.MimeType,
            DeclaredSizeBytes = request.DeclaredSizeBytes,
            DeclaredSha256 = request.DeclaredSha256.ToLowerInvariant(),
            StorageKey = storageKey,
            PresignedUrl = presigned.Url,
            ExpiresAtUtc = presigned.ExpiresAt,
            Status = UploadSessionStatus.Pending
        };
        await _sessionRepo.AddAsync(session, ct);

        return Result.Success(new UploadSessionDto(session.Id, presigned.Url, presigned.ExpiresAt));
    }
}

public class ConfirmUploadSessionHandler : IRequestHandler<ConfirmUploadSessionCommand, Result<AssetUploadConfirmedDto>>
{
    private readonly IAssetStorageService _storage;
    private readonly IAssetUploadSessionRepository _sessionRepo;
    private readonly IAssetRepository _assetRepo;
    private readonly IAssetVersionRepository _versionRepo;

    public ConfirmUploadSessionHandler(
        IAssetStorageService storage,
        IAssetUploadSessionRepository sessionRepo,
        IAssetRepository assetRepo,
        IAssetVersionRepository versionRepo)
    {
        _storage = storage;
        _sessionRepo = sessionRepo;
        _assetRepo = assetRepo;
        _versionRepo = versionRepo;
    }

    public async Task<Result<AssetUploadConfirmedDto>> Handle(ConfirmUploadSessionCommand request, CancellationToken ct)
    {
        var session = await _sessionRepo.GetPendingByIdAsync(request.SessionId, ct);
        if (session == null)
            return Result.Failure<AssetUploadConfirmedDto>("Session d'upload introuvable ou expirée.", "SESSION_NOT_FOUND");

        if (session.Status != UploadSessionStatus.Pending)
            return Result.Failure<AssetUploadConfirmedDto>("Cette session a déjà été traitée.", "SESSION_ALREADY_PROCESSED");

        if (DateTime.UtcNow > session.ExpiresAtUtc)
        {
            session.Status = UploadSessionStatus.Expired;
            _sessionRepo.Update(session);
            return Result.Failure<AssetUploadConfirmedDto>("La session d'upload a expiré.", "SESSION_EXPIRED");
        }

        // Verify file exists in MinIO and check size
        var metadata = await _storage.GetObjectMetadataAsync(session.StorageKey, ct);
        if (metadata == null)
        {
            session.Status = UploadSessionStatus.Failed;
            session.FailureReason = "Fichier introuvable dans le stockage.";
            _sessionRepo.Update(session);
            return Result.Failure<AssetUploadConfirmedDto>("Le fichier n'a pas été trouvé dans le stockage.", "FILE_NOT_FOUND");
        }

        // Verify size
        if (metadata.SizeBytes != request.ActualSizeBytes)
        {
            session.Status = UploadSessionStatus.Failed;
            session.FailureReason = $"Taille déclarée {request.ActualSizeBytes} ≠ taille réelle {metadata.SizeBytes}";
            _sessionRepo.Update(session);
            return Result.Failure<AssetUploadConfirmedDto>("La taille du fichier ne correspond pas.", "SIZE_MISMATCH");
        }

        // Verify SHA-256 — compare declared vs actual
        var actualSha = request.ActualSha256.ToLowerInvariant();
        var declaredSha = session.DeclaredSha256.ToLowerInvariant();
        if (actualSha != declaredSha)
        {
            session.Status = UploadSessionStatus.Failed;
            session.FailureReason = "Checksum SHA-256 invalide.";
            _sessionRepo.Update(session);
            return Result.Failure<AssetUploadConfirmedDto>("Le checksum SHA-256 du fichier ne correspond pas.", "CHECKSUM_MISMATCH");
        }

        // Create Asset + AssetVersion
        var asset = new Asset
        {
            WorkspaceId = session.WorkspaceId,
            ContentItemId = session.ContentItemId,
            MimeType = session.MimeType,
            OriginalFileName = session.FileName,
            CurrentVersionNumber = 1,
            CurrentStoragePath = session.StorageKey
        };
        await _assetRepo.AddAsync(asset, ct);

        var version = new AssetVersion
        {
            WorkspaceId = session.WorkspaceId,
            AssetId = asset.Id,
            VersionNumber = 1,
            StoragePath = session.StorageKey,
            FileSize = request.ActualSizeBytes,
            Checksum = actualSha,
            UploadedBy = session.InitiatedByUserId.ToString(),
            UploadedAtUtc = DateTime.UtcNow
        };
        await _versionRepo.AddAsync(version, ct);

        session.Status = UploadSessionStatus.Confirmed;
        session.ConfirmedAtUtc = DateTime.UtcNow;
        _sessionRepo.Update(session);

        return Result.Success(new AssetUploadConfirmedDto(
            asset.Id, session.StorageKey, 1, session.FileName));
    }
}

public class GetUploadSessionHandler : IRequestHandler<GetUploadSessionQuery, Result<UploadSessionDto>>
{
    private readonly IAssetUploadSessionRepository _sessionRepo;
    public GetUploadSessionHandler(IAssetUploadSessionRepository sessionRepo) => _sessionRepo = sessionRepo;

    public async Task<Result<UploadSessionDto>> Handle(GetUploadSessionQuery request, CancellationToken ct)
    {
        var session = await _sessionRepo.GetByIdAsync(request.SessionId, ct);
        if (session == null)
            return Result.Failure<UploadSessionDto>("Session introuvable.", "NOT_FOUND");

        return Result.Success(new UploadSessionDto(session.Id, session.PresignedUrl, session.ExpiresAtUtc));
    }
}
