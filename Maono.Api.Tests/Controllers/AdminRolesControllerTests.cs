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

public class AdminRolesControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AdminRolesController _controller;

    public AdminRolesControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AdminRolesController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public async Task ListRoles_ShouldReturnSuccess_WhenValid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListRolesQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<List<RoleDto>>(new List<RoleDto>()));

        var result = await _controller.ListRoles();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ListRoles_ShouldReturnError_WhenInvalid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListRolesQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<List<RoleDto>>("Erreur"));

        var result = await _controller.ListRoles();

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ListPermissions_ShouldReturnSuccess_WhenValid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListPermissionsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<List<PermissionDto>>(new List<PermissionDto>()));

        var result = await _controller.ListPermissions();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task ListPermissions_ShouldReturnError_WhenInvalid()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ListPermissionsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<List<PermissionDto>>("Erreur"));

        var result = await _controller.ListPermissions();

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateRole_ShouldReturnSuccess_WhenValid()
    {
        var command = new CreateRoleCommand("Role", null, new List<string>());
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<RoleDto>(new RoleDto(Guid.NewGuid(), "Role", null, false, new List<string>())));

        var result = await _controller.CreateRole(command);

        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateRole_ShouldReturnError_WhenInvalid()
    {
        var command = new CreateRoleCommand("Role", null, new List<string>());
        _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<RoleDto>("Erreur"));

        var result = await _controller.CreateRole(command);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UpdatePermissions_ShouldReturnSuccess_WhenValid()
    {
        var roleId = Guid.NewGuid();
        var request = new UpdatePermissionsRequest(new List<string>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateRolePermissionsCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<RoleDto>(new RoleDto(roleId, "Role", null, false, new List<string>())));

        var result = await _controller.UpdatePermissions(roleId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task UpdatePermissions_ShouldReturnError_WhenInvalid()
    {
        var roleId = Guid.NewGuid();
        var request = new UpdatePermissionsRequest(new List<string>());
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateRolePermissionsCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<RoleDto>("Erreur"));

        var result = await _controller.UpdatePermissions(roleId, request);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task DeleteRole_ShouldReturnSuccess_WhenValid()
    {
        var roleId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success());

        var result = await _controller.DeleteRole(roleId);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteRole_ShouldReturnError_WhenInvalid()
    {
        var roleId = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure("Erreur"));

        var result = await _controller.DeleteRole(roleId);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task AssignRole_ShouldReturnSuccess_WhenValid()
    {
        var userId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var request = new AssignRoleRequest("Role");
        _mediatorMock.Setup(m => m.Send(It.IsAny<AssignUserRoleCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Success<UserMembershipDto>(new UserMembershipDto(workspaceId, "Workspace", "Role", true, new List<string>())));

        var result = await _controller.AssignRole(userId, workspaceId, request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task AssignRole_ShouldReturnError_WhenInvalid()
    {
        var userId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var request = new AssignRoleRequest("Role");
        _mediatorMock.Setup(m => m.Send(It.IsAny<AssignUserRoleCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(Result.Failure<UserMembershipDto>("Erreur"));

        var result = await _controller.AssignRole(userId, workspaceId, request);

        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
    }
}
