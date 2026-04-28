using Maono.Api.Common;
using Maono.Application.Features.Assets.Commands;
using Maono.Application.Features.Assets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Contenus & Production")]
public class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AssetsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetAssetByIdQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("{id}/versions")]
    public async Task<IActionResult> GetVersions(Guid id)
    {
        var result = await _mediator.Send(new GetAssetVersionsQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Restore(Guid id, [FromBody] RestoreVersionRequest request)
    {
        var result = await _mediator.Send(new RestoreAssetVersionCommand(id, request.TargetVersionNumber));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    // ── Presigned Upload Flow (P1) ─────────────────────────────────────────────

    /// <summary>
    /// Step 1 — Initiate an upload session.
    /// Returns a presigned PUT URL valid 15 minutes for direct client-to-MinIO upload.
    /// Client must include x-amz-checksum-sha256 header when uploading.
    /// </summary>
    [HttpPost("upload/initiate")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> InitiateUpload([FromBody] InitiateUploadSessionCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Session d'upload créée"));
    }

    /// <summary>
    /// Step 2 — (Optional) Poll session status.
    /// </summary>
    [HttpGet("upload/sessions/{sessionId}")]
    public async Task<IActionResult> GetUploadSession(Guid sessionId)
    {
        var result = await _mediator.Send(new GetUploadSessionQuery(sessionId));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>
    /// Step 3 — Confirm upload after client has PUT the file.
    /// API verifies SHA-256 + size match, then creates Asset + AssetVersion.
    /// </summary>
    [HttpPost("upload/sessions/{sessionId}/confirm")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> ConfirmUpload(Guid sessionId, [FromBody] ConfirmUploadRequest request)
    {
        var result = await _mediator.Send(new ConfirmUploadSessionCommand(
            sessionId, request.ActualSizeBytes, request.ActualSha256));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Asset créé avec succès"));
    }

    /// <summary>ECR-020 — Search assets by filename, type, or content item.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? fileName, [FromQuery] string? assetType, [FromQuery] Guid? contentItemId)
    {
        var result = await _mediator.Send(new SearchAssetsQuery(fileName, assetType, contentItemId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>ECR-030 — Compare two versions of an asset side-by-side.</summary>
    [HttpGet("{id}/compare")]
    public async Task<IActionResult> CompareVersions(Guid id, [FromQuery] int versionA, [FromQuery] int versionB)
    {
        var result = await _mediator.Send(new CompareAssetVersionsQuery(id, versionA, versionB));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>ECR-031 — Storage quota usage and limits.</summary>
    [HttpGet("quota")]
    public async Task<IActionResult> GetQuota()
    {
        var result = await _mediator.Send(new Application.Features.Assets.GetStorageQuotaQuery());
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }
}

public record RestoreVersionRequest(int TargetVersionNumber);
public record ConfirmUploadRequest(long ActualSizeBytes, string ActualSha256);
