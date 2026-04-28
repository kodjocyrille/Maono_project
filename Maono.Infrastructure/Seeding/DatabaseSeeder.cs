using Maono.Domain.Identity.Entities;
using Maono.Infrastructure.Identity;
using Maono.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Maono.Infrastructure.Seeding;

/// <summary>
/// Seeds roles, permissions, and the bootstrap admin user on application startup.
/// Admin credentials are read from appsettings "AdminSeed" section.
/// </summary>
public static class DatabaseSeeder
{
    // ── System Permissions ───────────────────────────────────
    private static readonly (string Code, string Description)[] SystemPermissions =
    {
        // Campaigns
        ("campaigns.create", "Create campaigns"),
        ("campaigns.read", "View campaigns"),
        ("campaigns.update", "Edit campaigns"),
        ("campaigns.delete", "Delete campaigns"),

        // Content
        ("content.create", "Create content items"),
        ("content.read", "View content items"),
        ("content.update", "Edit content items"),
        ("content.delete", "Delete content items"),
        ("content.approve.internal", "Approve content internally"),
        ("content.approve.client", "Approve content as client"),

        // Clients
        ("clients.create", "Create client organizations"),
        ("clients.read", "View client organizations"),
        ("clients.update", "Edit client organizations"),
        ("clients.delete", "Delete client organizations"),

        // Planning
        ("planning.create", "Create calendar entries"),
        ("planning.read", "View calendar"),
        ("planning.update", "Edit calendar entries"),

        // Publications
        ("publications.schedule", "Schedule publications"),
        ("publications.publish", "Publish content"),
        ("publications.read", "View publications"),

        // Assets
        ("assets.upload", "Upload assets"),
        ("assets.read", "View assets"),
        ("assets.delete", "Delete assets"),

        // Missions
        ("missions.create", "Create missions"),
        ("missions.read", "View missions"),
        ("missions.update", "Edit missions"),
        ("missions.billing", "Manage mission billing"),

        // Approvals
        ("approvals.create", "Create approval cycles"),
        ("approvals.review", "Review and approve"),
        ("approvals.read", "View approval status"),

        // Performance
        ("performance.read", "View analytics"),
        ("performance.export", "Export reports"),

        // Notifications
        ("notifications.read", "View notifications"),
        ("notifications.manage", "Manage notification preferences"),

        // Workspace & Identity
        ("workspace.manage", "Manage workspace settings"),
        ("workspace.members", "Manage workspace members"),
        ("workspace.roles", "Manage roles and permissions"),
        ("workspace.billing", "Manage workspace billing"),

        // Admin
        ("admin.full", "Full administrative access"),
    };

    // ── System Roles ─────────────────────────────────────────
    private static readonly (string Name, string Description, string[] Permissions)[] SystemRoles =
    {
        ("Admin", "Full access to all features", new[]
        {
            "admin.full", "campaigns.create", "campaigns.read", "campaigns.update", "campaigns.delete",
            "content.create", "content.read", "content.update", "content.delete",
            "content.approve.internal", "content.approve.client",
            "clients.create", "clients.read", "clients.update", "clients.delete",
            "planning.create", "planning.read", "planning.update",
            "publications.schedule", "publications.publish", "publications.read",
            "assets.upload", "assets.read", "assets.delete",
            "missions.create", "missions.read", "missions.update", "missions.billing",
            "approvals.create", "approvals.review", "approvals.read",
            "performance.read", "performance.export",
            "notifications.read", "notifications.manage",
            "workspace.manage", "workspace.members", "workspace.roles", "workspace.billing"
        }),
        ("Strategist", "Campaign strategy and planning", new[]
        {
            "campaigns.create", "campaigns.read", "campaigns.update",
            "content.create", "content.read", "content.update",
            "clients.read", "clients.update",
            "planning.create", "planning.read", "planning.update",
            "publications.schedule", "publications.read",
            "approvals.create", "approvals.read",
            "performance.read", "performance.export",
            "notifications.read", "notifications.manage"
        }),
        ("Planner", "Content planning and scheduling", new[]
        {
            "campaigns.read",
            "content.create", "content.read", "content.update",
            "clients.read",
            "planning.create", "planning.read", "planning.update",
            "publications.schedule", "publications.read",
            "assets.upload", "assets.read",
            "notifications.read"
        }),
        ("Designer", "Asset creation and content design", new[]
        {
            "content.read", "content.update",
            "assets.upload", "assets.read", "assets.delete",
            "approvals.read",
            "notifications.read"
        }),
        ("ClientProxy", "External client access for approvals", new[]
        {
            "content.read",
            "content.approve.client",
            "approvals.review", "approvals.read",
            "assets.read",
            "notifications.read"
        }),
        ("FreelancerOwner", "Freelancer mode — missions and billing", new[]
        {
            "campaigns.create", "campaigns.read", "campaigns.update",
            "content.create", "content.read", "content.update",
            "clients.create", "clients.read", "clients.update",
            "planning.create", "planning.read", "planning.update",
            "publications.schedule", "publications.publish", "publications.read",
            "assets.upload", "assets.read",
            "missions.create", "missions.read", "missions.update", "missions.billing",
            "performance.read",
            "notifications.read", "notifications.manage",
            "workspace.manage"
        }),
        // ECR-006 — Rôles manquants CdCF §3.1
        ("Photographer", "Photography production — shoots, raw & edited assets", new[]
        {
            "content.read",
            "assets.upload", "assets.read",
            "missions.read", "missions.update",
            "approvals.read",
            "notifications.read"
        }),
        ("SocialMediaManager", "Social media publishing and monitoring", new[]
        {
            "content.read",
            "publications.schedule", "publications.publish", "publications.read",
            "planning.read",
            "performance.read",
            "assets.read",
            "notifications.read"
        }),
        ("Collaborator", "External collaborator with scoped task access", new[]
        {
            "content.read", "content.update",
            "assets.upload", "assets.read",
            "missions.read",
            "notifications.read"
        }),
    };

    /// <summary>
    /// Seeds all system data. Called once at startup.
    /// Idempotent — safe to run multiple times.
    /// Fast-path: skips entirely if permissions, roles and admin already exist.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MaonoDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MaonoDbContext>>();

        // ── Fast-path: skip if already fully seeded ──
        var permCount = await context.Permissions.CountAsync();
        var roleCount = await context.DomainRoles.CountAsync();
        var adminEmail = config["AdminSeed:Email"];
        var adminExists = !string.IsNullOrWhiteSpace(adminEmail) &&
                          await context.DomainUsers
                              .Include(u => u.Memberships)
                              .AnyAsync(u => u.Email == adminEmail && u.Memberships.Any());

        if (permCount >= SystemPermissions.Length && roleCount >= SystemRoles.Length && adminExists)
        {
            logger.LogDebug("Database already seeded — skipping");
            return;
        }

        logger.LogInformation("Seeding database (permissions: {PermCount}/{ExpectedPerms}, roles: {RoleCount}/{ExpectedRoles}, admin: {AdminExists})",
            permCount, SystemPermissions.Length, roleCount, SystemRoles.Length, adminExists);

        await SeedPermissionsAsync(context, logger);
        await SeedRolesAsync(context, logger);
        await SeedAdminAsync(context, userManager, config, logger);
    }

    private static async Task SeedPermissionsAsync(MaonoDbContext context, ILogger logger)
    {
        var existingCodes = await context.Permissions.Select(p => p.Code).ToListAsync();

        var newPerms = SystemPermissions
            .Where(p => !existingCodes.Contains(p.Code))
            .Select(p => new Permission { Code = p.Code, Description = p.Description })
            .ToList();

        if (newPerms.Any())
        {
            await context.Permissions.AddRangeAsync(newPerms);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} permissions", newPerms.Count);
        }
    }

    private static async Task SeedRolesAsync(MaonoDbContext context, ILogger logger)
    {
        var allPermissions = await context.Permissions.ToListAsync();
        var existingRoles = await context.DomainRoles.Select(r => r.Name).ToListAsync();

        foreach (var (name, description, permissionCodes) in SystemRoles)
        {
            if (existingRoles.Contains(name)) continue;

            var role = new Role
            {
                Name = name,
                Description = description,
                IsSystem = true,
                Permissions = allPermissions.Where(p => permissionCodes.Contains(p.Code)).ToList()
            };
            context.DomainRoles.Add(role);
            logger.LogInformation("Seeded role: {RoleName}", name);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(
        MaonoDbContext context,
        UserManager<ApplicationUser> userManager,
        IConfiguration config,
        ILogger logger)
    {
        var adminEmail = config["AdminSeed:Email"];
        var adminPassword = config["AdminSeed:Password"];
        var adminDisplayName = config["AdminSeed:DisplayName"] ?? "Super Admin";

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("AdminSeed section missing in appsettings — skipping admin bootstrap");
            return;
        }

        var adminRole = await context.DomainRoles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            logger.LogError("Admin role not found in database — cannot seed admin user");
            return;
        }

        // Check if admin already exists
        var existingUser = await context.DomainUsers
            .Include(u => u.Memberships)
            .FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (existingUser != null)
        {
            // ── REPAIR: ensure admin has a workspace membership with Admin role ──
            if (!existingUser.Memberships.Any())
            {
                logger.LogWarning("Admin user exists but has NO membership — repairing...");

                // Ensure default workspace exists
                var workspace = await context.Workspaces.FirstOrDefaultAsync(w => w.Slug == "maono-hq");
                if (workspace == null)
                {
                    workspace = new Workspace { Name = "Maono HQ", Slug = "maono-hq" };
                    context.Workspaces.Add(workspace);
                }

                var membership = new WorkspaceMembership
                {
                    UserId = existingUser.Id,
                    WorkspaceId = workspace.Id,
                    RoleId = adminRole.Id,
                    IsDefault = true,
                };
                context.WorkspaceMemberships.Add(membership);
                await context.SaveChangesAsync();
                logger.LogInformation("Admin membership repaired — role: Admin, workspace: {Workspace}", workspace.Name);
            }
            else if (!existingUser.Memberships.Any(m => m.RoleId == adminRole.Id))
            {
                // Has membership but wrong role — fix it
                var membership = existingUser.Memberships.First();
                membership.RoleId = adminRole.Id;
                await context.SaveChangesAsync();
                logger.LogInformation("Admin membership role corrected to Admin");
            }
            else
            {
                logger.LogInformation("Admin user already exists with correct role: {Email}", adminEmail);
            }
            return;
        }

        // ── CREATE: brand new admin user ──
        var identityUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, DisplayName = adminDisplayName };
        var identityResult = await userManager.CreateAsync(identityUser, adminPassword);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
            logger.LogError("Failed to create admin Identity user: {Errors}", errors);
            return;
        }

        // Create domain User
        var adminUser = new User
        {
            IdentityId = identityUser.Id,
            Email = adminEmail,
            DisplayName = adminDisplayName,
        };
        context.DomainUsers.Add(adminUser);

        // Create default workspace
        var defaultWorkspace = new Workspace
        {
            Name = "Maono HQ",
            Slug = "maono-hq",
        };
        context.Workspaces.Add(defaultWorkspace);

        // Assign Admin role
        var adminMembership = new WorkspaceMembership
        {
            UserId = adminUser.Id,
            WorkspaceId = defaultWorkspace.Id,
            RoleId = adminRole.Id,
            IsDefault = true,
        };
        context.WorkspaceMemberships.Add(adminMembership);

        await context.SaveChangesAsync();
        logger.LogInformation("Admin bootstrap complete — email: {Email}, workspace: {Workspace}", adminEmail, defaultWorkspace.Name);
    }
}
