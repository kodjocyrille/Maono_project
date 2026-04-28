using Maono.Application.Common.Models;
using Maono.Application.Features.Admin.Commands;
using Maono.Application.Features.Admin.Queries;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Repository;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Maono.Application.Features.Admin.Handlers;

// ── QUERIES ──────────────────────────────────────────────

public class ListRolesHandler : IRequestHandler<ListRolesQuery, Result<List<RoleDto>>>
{
    private readonly IRoleRepository _roleRepo;

    public ListRolesHandler(IRoleRepository roleRepo) => _roleRepo = roleRepo;

    public async Task<Result<List<RoleDto>>> Handle(ListRolesQuery request, CancellationToken ct)
    {
        var roles = await _roleRepo.GetAllWithPermissionsAsync(ct);

        var dtos = roles.Select(r => new RoleDto(
            r.Id, r.Name, r.Description, r.IsSystem,
            r.Permissions.Select(p => p.Code).OrderBy(c => c).ToList()
        )).OrderBy(r => r.Name).ToList();

        return Result.Success(dtos);
    }
}

public class ListPermissionsHandler : IRequestHandler<ListPermissionsQuery, Result<List<PermissionDto>>>
{
    private readonly IPermissionRepository _permRepo;

    public ListPermissionsHandler(IPermissionRepository permRepo) => _permRepo = permRepo;

    public async Task<Result<List<PermissionDto>>> Handle(ListPermissionsQuery request, CancellationToken ct)
    {
        var permissions = await _permRepo.GetAllAsync(ct);

        var dtos = permissions
            .Select(p => new PermissionDto(p.Id, p.Code, p.Description))
            .OrderBy(p => p.Code)
            .ToList();

        return Result.Success(dtos);
    }
}

// ── COMMANDS ──────────────────────────────────────────────

public class CreateRoleHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepo;
    private readonly IPermissionRepository _permRepo;

    public CreateRoleHandler(IRoleRepository roleRepo, IPermissionRepository permRepo)
    {
        _roleRepo = roleRepo;
        _permRepo = permRepo;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken ct)
    {
        // Check name uniqueness
        var existing = await _roleRepo.GetByNameAsync(request.Name, ct);
        if (existing != null)
            return Result.Failure<RoleDto>($"Un rôle avec le nom '{request.Name}' existe déjà.", "DUPLICATE_ROLE");

        // Validate permissions
        var allPermissions = await _permRepo.GetAllAsync(ct);
        var matchedPermissions = allPermissions
            .Where(p => request.PermissionCodes.Contains(p.Code))
            .ToList();

        var unknownCodes = request.PermissionCodes
            .Except(matchedPermissions.Select(p => p.Code))
            .ToList();

        if (unknownCodes.Any())
            return Result.Failure<RoleDto>(
                $"Permissions inconnues : {string.Join(", ", unknownCodes)}", "INVALID_PERMISSIONS");

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            IsSystem = false,
            Permissions = matchedPermissions
        };

        await _roleRepo.AddAsync(role, ct);

        return Result.Success(new RoleDto(
            role.Id, role.Name, role.Description, role.IsSystem,
            role.Permissions.Select(p => p.Code).OrderBy(c => c).ToList()
        ));
    }
}

public class UpdateRolePermissionsHandler : IRequestHandler<UpdateRolePermissionsCommand, Result<RoleDto>>
{
    private readonly IRoleRepository _roleRepo;
    private readonly IPermissionRepository _permRepo;

    public UpdateRolePermissionsHandler(IRoleRepository roleRepo, IPermissionRepository permRepo)
    {
        _roleRepo = roleRepo;
        _permRepo = permRepo;
    }

    public async Task<Result<RoleDto>> Handle(UpdateRolePermissionsCommand request, CancellationToken ct)
    {
        var role = await _roleRepo.GetByIdWithPermissionsAsync(request.RoleId, ct);
        if (role == null)
            return Result.Failure<RoleDto>("Rôle introuvable.", "NOT_FOUND");

        // Validate permissions
        var allPermissions = await _permRepo.GetAllAsync(ct);
        var matchedPermissions = allPermissions
            .Where(p => request.PermissionCodes.Contains(p.Code))
            .ToList();

        var unknownCodes = request.PermissionCodes
            .Except(matchedPermissions.Select(p => p.Code))
            .ToList();

        if (unknownCodes.Any())
            return Result.Failure<RoleDto>(
                $"Permissions inconnues : {string.Join(", ", unknownCodes)}", "INVALID_PERMISSIONS");

        // Replace permissions
        role.Permissions.Clear();
        foreach (var perm in matchedPermissions)
            role.Permissions.Add(perm);

        _roleRepo.Update(role);

        return Result.Success(new RoleDto(
            role.Id, role.Name, role.Description, role.IsSystem,
            role.Permissions.Select(p => p.Code).OrderBy(c => c).ToList()
        ));
    }
}

public class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, Result>
{
    private readonly IRoleRepository _roleRepo;

    public DeleteRoleHandler(IRoleRepository roleRepo) => _roleRepo = roleRepo;

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken ct)
    {
        var role = await _roleRepo.GetByIdAsync(request.RoleId, ct);
        if (role == null)
            return Result.Failure("Rôle introuvable.", "NOT_FOUND");

        if (role.IsSystem)
            return Result.Failure("Impossible de supprimer un rôle système.", "SYSTEM_ROLE");

        _roleRepo.Remove(role);
        return Result.Success();
    }
}

public class AssignUserRoleHandler : IRequestHandler<AssignUserRoleCommand, Result<UserMembershipDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IMembershipRepository _membershipRepo;

    public AssignUserRoleHandler(
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IMembershipRepository membershipRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _membershipRepo = membershipRepo;
    }

    public async Task<Result<UserMembershipDto>> Handle(AssignUserRoleCommand request, CancellationToken ct)
    {
        // 1. Validate user exists
        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user == null)
            return Result.Failure<UserMembershipDto>("Utilisateur introuvable.", "USER_NOT_FOUND");

        // 2. Validate role exists
        var role = await _roleRepo.GetByNameAsync(request.RoleName, ct);
        if (role == null)
            return Result.Failure<UserMembershipDto>($"Rôle '{request.RoleName}' introuvable.", "ROLE_NOT_FOUND");

        // 3. Check if user already has a membership in this workspace
        var existingMembership = await _membershipRepo.GetByUserAndWorkspaceAsync(
            request.UserId, request.WorkspaceId, ct);

        if (existingMembership != null)
        {
            // Update existing membership's role
            existingMembership.RoleId = role.Id;
            _membershipRepo.Update(existingMembership);

            return Result.Success(new UserMembershipDto(
                existingMembership.WorkspaceId,
                "", // will be resolved by caller
                role.Name,
                existingMembership.IsDefault,
                role.Permissions.Select(p => p.Code).ToList()
            ));
        }
        else
        {
            // Create new membership
            var isFirst = !(await _membershipRepo.GetByUserIdAsync(request.UserId, ct)).Any();

            var membership = new WorkspaceMembership
            {
                UserId = request.UserId,
                WorkspaceId = request.WorkspaceId,
                RoleId = role.Id,
                IsDefault = isFirst, // First membership is default
            };
            await _membershipRepo.AddAsync(membership, ct);

            return Result.Success(new UserMembershipDto(
                membership.WorkspaceId,
                "",
                role.Name,
                membership.IsDefault,
                role.Permissions.Select(p => p.Code).ToList()
            ));
        }
    }
}
