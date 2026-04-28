using Maono.Application.Common.Interfaces;
using Maono.Application.Common.Models;
using Maono.Application.Features.Auth.Queries;
using Maono.Domain.Identity.Entities;
using Maono.Infrastructure.Identity;
using Maono.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MaonoDbContext _context;

    public AuthenticationService(UserManager<ApplicationUser> userManager, MaonoDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<Result<User>> RegisterAsync(string email, string password, string displayName, CancellationToken ct = default)
        => await RegisterAsync(email, password, displayName, null, null, null, ct);

    public async Task<Result<User>> RegisterAsync(
        string email, string password, string firstName, string lastName, string? phoneNumber,
        string? displayName = null, CancellationToken ct = default)
    {
        var fullName = displayName ?? $"{firstName} {lastName}".Trim();

        var identityUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = fullName
        };

        var result = await _userManager.CreateAsync(identityUser, password);
        if (!result.Succeeded)
            return Result.Failure<User>(string.Join("; ", result.Errors.Select(e => e.Description)));

        var domainUser = new User
        {
            IdentityId = identityUser.Id,
            Email = email,
            FirstName = firstName ?? "",
            LastName = lastName ?? "",
            PhoneNumber = phoneNumber,
            DisplayName = fullName
        };

        _context.DomainUsers.Add(domainUser);
        identityUser.DomainUserId = domainUser.Id;
        await _userManager.UpdateAsync(identityUser);

        return Result.Success(domainUser);
    }

    public async Task<Result<User>> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        var identityUser = await _userManager.FindByEmailAsync(email);
        if (identityUser == null || !await _userManager.CheckPasswordAsync(identityUser, password))
            return Result.Failure<User>("Identifiant ou mot de passe incorrect.");

        var domainUser = await _context.DomainUsers
            .Include(u => u.Memberships)
                .ThenInclude(m => m.Role)
            .FirstOrDefaultAsync(u => u.IdentityId == identityUser.Id, ct);

        return domainUser == null
            ? Result.Failure<User>("Utilisateur introuvable.")
            : Result.Success(domainUser);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, Guid? roleId, CancellationToken ct = default)
    {
        if (roleId == null) return new List<string>();

        var role = await _context.DomainRoles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, ct);

        return role?.Permissions.Select(p => p.Code).ToList() ?? new List<string>();
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.DomainUsers.FindAsync(new object[] { userId }, ct);
    }

    public async Task<User?> GetUserWithMembershipsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.DomainUsers
            .Include(u => u.Memberships)
                .ThenInclude(m => m.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task UpdateLastLoginAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _context.DomainUsers.FindAsync(new object[] { userId }, ct);
        if (user != null)
            user.LastLoginAtUtc = DateTime.UtcNow;
    }

    // Sessions
    public async Task CreateSessionAsync(DeviceSession session, CancellationToken ct = default)
    {
        _context.Set<DeviceSession>().Add(session);
        await Task.CompletedTask;
    }

    public async Task<List<SessionDto>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.Set<DeviceSession>()
            .Where(s => s.UserId == userId && !s.IsRevoked && s.LogoutAtUtc == null)
            .OrderByDescending(s => s.LastActiveAtUtc)
            .Select(s => new SessionDto(
                s.Id, s.DeviceType, s.DeviceName, s.IpAddress,
                s.LoginAtUtc, s.LastActiveAtUtc, false))
            .ToListAsync(ct);
    }

    public async Task RevokeSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        var session = await _context.Set<DeviceSession>().FindAsync(new object[] { sessionId }, ct);
        if (session != null)
        {
            session.IsRevoked = true;
            session.LogoutAtUtc = DateTime.UtcNow;

            // Revoke all refresh tokens for this session
            var tokens = await _context.RefreshTokens
                .Where(t => t.DeviceSessionId == sessionId && t.RevokedAtUtc == null)
                .ToListAsync(ct);
            foreach (var token in tokens)
            {
                token.RevokedAtUtc = DateTime.UtcNow;
                token.RevokedReason = "Session revoked";
            }
        }
    }

    public async Task RevokeAllSessionsExceptAsync(Guid userId, Guid keepSessionId, CancellationToken ct = default)
    {
        var sessions = await _context.Set<DeviceSession>()
            .Where(s => s.UserId == userId && s.Id != keepSessionId && !s.IsRevoked)
            .ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.IsRevoked = true;
            session.LogoutAtUtc = DateTime.UtcNow;
        }

        // Revoke all tokens not in the kept session
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.DeviceSessionId != keepSessionId && t.RevokedAtUtc == null)
            .ToListAsync(ct);
        foreach (var token in tokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedReason = "All other sessions revoked";
        }
    }

    public async Task UpdateSessionLastActiveAsync(Guid sessionId, string? ipAddress, CancellationToken ct = default)
    {
        var session = await _context.Set<DeviceSession>().FindAsync(new object[] { sessionId }, ct);
        if (session != null)
        {
            session.LastActiveAtUtc = DateTime.UtcNow;
            if (ipAddress != null)
                session.IpAddress = ipAddress;
        }
    }

    // Refresh tokens
    public async Task SaveRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        _context.RefreshTokens.Add(refreshToken);
        await Task.CompletedTask;
    }

    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token && t.RevokedAtUtc == null, ct);

        return refreshToken?.IsExpired == true ? null : refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken token, string reason, CancellationToken ct = default)
    {
        token.RevokedAtUtc = DateTime.UtcNow;
        token.RevokedReason = reason;
        await Task.CompletedTask;
    }
}
