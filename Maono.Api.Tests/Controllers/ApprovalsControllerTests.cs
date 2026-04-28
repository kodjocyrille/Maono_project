using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Common;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Approvals.Commands;
using Maono.Application.Features.Approvals.DTOs;
using Maono.Application.Features.Approvals.Queries;
using Maono.Domain.Approval.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class ApprovalsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ApprovalsController _controller;

    public ApprovalsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ApprovalsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task SubmitInternal_ShouldReturnSuccess_WhenValid()
    {
        var command = new SubmitInternalApprovalCommand(Guid.NewGuid(), ApprovalStatus.Approved, "Commentaire");
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<ApprovalDecisionDto>(
                         new ApprovalDecisionDto(Guid.NewGuid(), ActorType.InternalUser, Guid.NewGuid(), ApprovalStatus.Approved, "Commentaire", DateTime.UtcNow)));

        var result = await _controller.SubmitInternal(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task SubmitInternal_ShouldReturnError_WhenInvalid()
    {
        var command = new SubmitInternalApprovalCommand(Guid.NewGuid(), ApprovalStatus.Approved, "Commentaire");
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<ApprovalDecisionDto>("Erreur"));

        var result = await _controller.SubmitInternal(command);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task SubmitClient_ShouldReturnSuccess_WhenValid()
    {
        // SubmitClientApprovalCommand has 3 params: ContentItemId, Decision, Comment
        var command = new SubmitClientApprovalCommand(Guid.NewGuid(), ApprovalStatus.Approved, "Commentaire");
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<ApprovalDecisionDto>(
                         new ApprovalDecisionDto(Guid.NewGuid(), ActorType.ClientContact, null, ApprovalStatus.Approved, "Commentaire", DateTime.UtcNow)));

        var result = await _controller.SubmitClient(command);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task SubmitClient_ShouldReturnError_WhenInvalid()
    {
        var command = new SubmitClientApprovalCommand(Guid.NewGuid(), ApprovalStatus.Approved, "Commentaire");
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<ApprovalDecisionDto>("Erreur"));

        var result = await _controller.SubmitClient(command);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetCycles_ShouldReturnSuccess_WhenValid()
    {
        var contentId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetApprovalCyclesQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<List<ApprovalCycleDetailDto>>(new List<ApprovalCycleDetailDto>()));

        var result = await _controller.GetCycles(contentId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetCycles_ShouldReturnError_WhenInvalid()
    {
        var contentId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetApprovalCyclesQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<List<ApprovalCycleDetailDto>>("Erreur"));

        var result = await _controller.GetCycles(contentId);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }
}
