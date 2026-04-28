using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Admin.Commands;
using Maono.Application.Features.Admin.Queries;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Identity.Enums;
using Maono.Domain.Identity.Repository;
using MediatR;

namespace Maono.Application.Features.Admin.Handlers;

public class ListUsersHandler : IRequestHandler<ListUsersQuery, Result<List<UserAdminDto>>>
{
    private readonly IUserRepository _userRepo;

    public ListUsersHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<Result<List<UserAdminDto>>> Handle(ListUsersQuery request, CancellationToken ct)
    {
        var users = await _userRepo.GetAllWithMembershipsAsync(ct);

        var query = users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(u =>
                u.Email.Contains(request.Search, StringComparison.OrdinalIgnoreCase) ||
                u.DisplayName.Contains(request.Search, StringComparison.OrdinalIgnoreCase));

        if (request.ActiveOnly)
            query = query.Where(u => u.Status == UserStatus.Active && !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.RoleFilter))
            query = query.Where(u => u.Memberships.Any(m =>
                m.Role != null && m.Role.Name.Equals(request.RoleFilter, StringComparison.OrdinalIgnoreCase)));

        var dtos = query.Select(u => MapToDto(u)).ToList();

        return Result.Success(dtos);
    }

    private static UserAdminDto MapToDto(User u) => new(
        u.Id,
        u.Email,
        u.DisplayName,
        u.Status == UserStatus.Active,
        u.LastLoginAtUtc,
        u.CreatedAtUtc,
        u.Memberships.Select(m => new UserMembershipDto(
            m.WorkspaceId,
            m.Workspace?.Name ?? "",
            m.Role?.Name ?? "",
            m.IsDefault,
            m.Role?.Permissions.Select(p => p.Code).ToList() ?? new List<string>()
        )).ToList()
    );
}

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<UserAdminDto>>
{
    private readonly IAuthenticationService _authService;
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IMembershipRepository _membershipRepo;

    public CreateUserHandler(
        IAuthenticationService authService,
        IUserRepository userRepo,
        IRoleRepository roleRepo,
        IMembershipRepository membershipRepo)
    {
        _authService = authService;
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _membershipRepo = membershipRepo;
    }

    public async Task<Result<UserAdminDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // 1. Check if email already exists
        var existing = await _userRepo.GetByEmailAsync(request.Email, ct);
        if (existing != null)
            return Result.Failure<UserAdminDto>("Un utilisateur avec cet email existe déjà.", "DUPLICATE_EMAIL");

        // 2. Validate role exists
        var role = await _roleRepo.GetByNameAsync(request.RoleName, ct);
        if (role == null)
            return Result.Failure<UserAdminDto>($"Rôle '{request.RoleName}' introuvable.", "ROLE_NOT_FOUND");

        // 3. Register user via Identity
        var password = request.Password ?? GenerateTemporaryPassword();
        var registerResult = await _authService.RegisterAsync(request.Email, password, request.DisplayName, ct);
        if (!registerResult.IsSuccess)
            return Result.Failure<UserAdminDto>(registerResult.Error!);

        var user = registerResult.Value!;

        // 4. Get default workspace (first available)
        var memberships = await _membershipRepo.GetByUserIdAsync(user.Id, ct);
        // If no membership yet, we need to find the admin's workspace
        // For now, we'll get any workspace from the system
        var workspace = await _membershipRepo.GetFirstWorkspaceAsync(ct);
        if (workspace != null)
        {
            var membership = new WorkspaceMembership
            {
                UserId = user.Id,
                WorkspaceId = workspace.Id,
                RoleId = role.Id,
                IsDefault = true,
            };
            await _membershipRepo.AddAsync(membership, ct);
        }

        // 5. Reload with memberships for response
        var reloaded = await _userRepo.GetAllWithMembershipsAsync(ct);
        var userWithMemberships = reloaded.FirstOrDefault(u => u.Id == user.Id);

        return Result.Success(new UserAdminDto(
            user.Id, user.Email, user.DisplayName,
            true, null, user.CreatedAtUtc,
            userWithMemberships?.Memberships.Select(m => new UserMembershipDto(
                m.WorkspaceId, m.Workspace?.Name ?? "", m.Role?.Name ?? "",
                m.IsDefault,
                m.Role?.Permissions.Select(p => p.Code).ToList() ?? new List<string>()
            )).ToList() ?? new List<UserMembershipDto>()
        ));
    }

    private static string GenerateTemporaryPassword()
        => $"Temp@{Guid.NewGuid().ToString("N")[..8]}!";
}

public class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Result<UserAdminDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly IMembershipRepository _membershipRepo;

    public UpdateUserHandler(IUserRepository userRepo, IRoleRepository roleRepo, IMembershipRepository membershipRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _membershipRepo = membershipRepo;
    }

    public async Task<Result<UserAdminDto>> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await _userRepo.GetWithMembershipsAsync(request.UserId, ct);
        if (user == null)
            return Result.Failure<UserAdminDto>("Utilisateur introuvable.", "NOT_FOUND");

        // Update status
        if (request.IsActive.HasValue)
            user.Status = request.IsActive.Value ? UserStatus.Active : UserStatus.Inactive;

        // Update role on default membership
        if (!string.IsNullOrWhiteSpace(request.RoleName))
        {
            var role = await _roleRepo.GetByNameAsync(request.RoleName, ct);
            if (role == null)
                return Result.Failure<UserAdminDto>($"Rôle '{request.RoleName}' introuvable.", "ROLE_NOT_FOUND");

            var defaultMembership = user.Memberships.FirstOrDefault(m => m.IsDefault)
                ?? user.Memberships.FirstOrDefault();

            if (defaultMembership != null)
            {
                defaultMembership.RoleId = role.Id;
            }
        }

        _userRepo.Update(user);

        // Reload with full data
        var reloaded = await _userRepo.GetAllWithMembershipsAsync(ct);
        var updated = reloaded.First(u => u.Id == user.Id);

        return Result.Success(new UserAdminDto(
            updated.Id, updated.Email, updated.DisplayName,
            updated.Status == UserStatus.Active,
            updated.LastLoginAtUtc, updated.CreatedAtUtc,
            updated.Memberships.Select(m => new UserMembershipDto(
                m.WorkspaceId, m.Workspace?.Name ?? "", m.Role?.Name ?? "",
                m.IsDefault,
                m.Role?.Permissions.Select(p => p.Code).ToList() ?? new List<string>()
            )).ToList()
        ));
    }
}

public class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand, Result>
{
    private readonly IUserRepository _userRepo;

    public DeactivateUserHandler(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken ct)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId, ct);
        if (user == null) return Result.Failure("Utilisateur introuvable.", "NOT_FOUND");

        user.Status = UserStatus.Inactive;
        _userRepo.Update(user);
        return Result.Success();
    }
}
