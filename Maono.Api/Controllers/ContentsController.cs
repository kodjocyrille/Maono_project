using Maono.Api.Common;
using Maono.Application.Features.Content.Commands;
using Maono.Application.Features.Content.Queries;
using Maono.Domain.Content.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maono.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Contenus & Production")]
public class ContentsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ContentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Create([FromBody] CreateContentCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Contenu créé"));
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status)
    {
        var result = await _mediator.Send(new ListContentQuery(status));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _mediator.Send(new GetContentByIdQuery(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpGet("deadline")]
    public async Task<IActionResult> GetByDeadline([FromQuery] DateTime deadline)
    {
        var result = await _mediator.Send(new GetContentByDeadlineQuery(deadline));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContentCommand command)
    {
        if (id != command.Id) return BadRequest(ApiResponse<object>.Error("L'identifiant ne correspond pas.", 400));
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Contenu mis à jour"));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ContentStatusRequest request)
    {
        var result = await _mediator.Send(new UpdateContentStatusCommand(id, request.Status));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Statut mis à jour"));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteContentCommand(id));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Contenu archivé"));
    }

    // ── ECR-011 — Content Dependencies ──────────────────────

    /// <summary>Add a blocking dependency between two content items.</summary>
    [HttpPost("{id}/dependencies")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> AddDependency(Guid id, [FromBody] AddDependencyRequest request)
    {
        var result = await _mediator.Send(new AddContentDependencyCommand(id, request.BlockingContentId, request.DependencyType));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse.Ok("Dépendance ajoutée"));
    }

    /// <summary>Remove a content dependency.</summary>
    [HttpDelete("dependencies/{dependencyId}")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> RemoveDependency(Guid dependencyId)
    {
        var result = await _mediator.Send(new RemoveContentDependencyCommand(dependencyId));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Dépendance supprimée"));
    }

    // ── ECR-017 — Content Annotations ───────────────────────

    /// <summary>Create a visual annotation on an asset version.</summary>
    [HttpPost("annotations")]
    public async Task<IActionResult> CreateAnnotation([FromBody] CreateAnnotationRequest request)
    {
        var result = await _mediator.Send(new CreateAnnotationCommand(request.AssetVersionId, request.CoordinatesJson, request.Body));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return StatusCode(201, ApiResponse<object>.Created(result.Value!, "Annotation créée"));
    }

    /// <summary>Delete an annotation.</summary>
    [HttpDelete("annotations/{annotationId}")]
    public async Task<IActionResult> DeleteAnnotation(Guid annotationId)
    {
        var result = await _mediator.Send(new DeleteAnnotationCommand(annotationId));
        if (!result.IsSuccess) return NotFound(ApiResponse<object>.Error(result.Error!, 404));
        return Ok(ApiResponse.Ok("Annotation supprimée"));
    }

    // ── ECR-033 — Full-text search ────────────────────────

    /// <summary>Search content items with filters.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] string? status, [FromQuery] string? format, [FromQuery] int? minPriority)
    {
        var result = await _mediator.Send(new Application.Features.Content.SearchContentsQuery(keyword, status, format, minPriority));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    // ── ECR-035 — Auto-archive ────────────────────────────

    /// <summary>Archive content published more than N days ago.</summary>
    [HttpPost("archive-old")]
    [Authorize(Policy = "CanManageContent")]
    public async Task<IActionResult> ArchiveOld([FromQuery] int days = 90)
    {
        var result = await _mediator.Send(new Application.Features.Content.ArchiveOldContentsCommand(days));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(new { ArchivedCount = result.Value }, $"{result.Value} contenus archivés"));
    }

    // ── ECR-036/037 — RGPD ──────────────────────────────

    /// <summary>Export all data associated with a user (RGPD).</summary>
    [HttpGet("rgpd/export/{userId}")]
    [Authorize(Policy = "CanManageWorkspace")]
    public async Task<IActionResult> ExportUserData(Guid userId)
    {
        var result = await _mediator.Send(new Application.Features.Content.ExportUserDataQuery(userId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(result.Value!));
    }

    /// <summary>Purge user notifications (RGPD right to erasure).</summary>
    [HttpDelete("rgpd/purge/{userId}")]
    [Authorize(Policy = "CanManageWorkspace")]
    public async Task<IActionResult> PurgeUserData(Guid userId)
    {
        var result = await _mediator.Send(new Application.Features.Content.PurgeUserDataCommand(userId));
        if (!result.IsSuccess) return BadRequest(ApiResponse<object>.Error(result.Error!, 400));
        return Ok(ApiResponse<object>.Ok(new { PurgedCount = result.Value }));
    }
}

public record ContentStatusRequest(ContentStatus Status);
public record AddDependencyRequest(Guid BlockingContentId, string? DependencyType);
public record CreateAnnotationRequest(Guid AssetVersionId, string? CoordinatesJson, string Body);
