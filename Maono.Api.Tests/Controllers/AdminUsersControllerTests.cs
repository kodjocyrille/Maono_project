using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Maono.Api.Common;
using Maono.Api.Controllers;
using Maono.Application.Common.Models;
using Maono.Application.Features.Admin;
using Maono.Application.Features.Admin.Commands;
using Maono.Application.Features.Admin.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Maono.Api.Tests.Controllers;

public class AdminUsersControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AdminUsersController _controller;

    public AdminUsersControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AdminUsersController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task List_ShouldReturnSuccess_WhenValid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListUsersQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<List<UserAdminDto>>(new List<UserAdminDto>()));

        var result = await _controller.List(null, null, true);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task List_ShouldReturnError_WhenInvalid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListUsersQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<List<UserAdminDto>>("Erreur"));

        var result = await _controller.List(null, null, true);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Update_ShouldReturnSuccess_WhenValid()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("Role", true);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<UserAdminDto>(
                         new UserAdminDto(userId, "test@mail.com", "Test User", true, null, DateTime.UtcNow, new List<UserMembershipDto>())));

        var result = await _controller.Update(userId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_ShouldReturnError_WhenInvalid()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest("Role", true);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<UserAdminDto>("Erreur"));

        var result = await _controller.Update(userId, request);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnSuccess_WhenValid()
    {
        var userId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeactivateUserCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success());

        var result = await _controller.Deactivate(userId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Deactivate_ShouldReturnError_WhenInvalid()
    {
        var userId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeactivateUserCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure("Erreur"));

        var result = await _controller.Deactivate(userId);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }
}
