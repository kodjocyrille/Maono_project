using Maono.Domain.Common;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Assets.Entities;
using Maono.Domain.Audit.Entities;
using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Clients.Entities;
using Maono.Domain.Content.Entities;
using Maono.Domain.Identity.Entities;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Notifications.Entities;
using Maono.Domain.Performance.Entities;
using Maono.Domain.Planning.Entities;
using Maono.Domain.Publications.Entities;
using Maono.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Maono.Infrastructure.Persistence;

public class MaonoDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly Guid? _currentWorkspaceId;

    public MaonoDbContext(DbContextOptions<MaonoDbContext> options) : base(options)
    {
    }

    public MaonoDbContext(DbContextOptions<MaonoDbContext> options, Guid? currentWorkspaceId)
        : base(options)
    {
        _currentWorkspaceId = currentWorkspaceId;
    }

    // Identity & Tenancy
    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMembership> WorkspaceMemberships => Set<WorkspaceMembership>();
    public DbSet<Role> DomainRoles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DeviceSession> DeviceSessions => Set<DeviceSession>();
    public DbSet<ClientAccessToken> ClientAccessTokens => Set<ClientAccessToken>();

    // Clients
    public DbSet<ClientOrganization> ClientOrganizations => Set<ClientOrganization>();
    public DbSet<ClientContact> ClientContacts => Set<ClientContact>();
    public DbSet<BrandProfile> BrandProfiles => Set<BrandProfile>();

    // Campaigns
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignKpi> CampaignKpis => Set<CampaignKpi>();
    public DbSet<CampaignTag> CampaignTags => Set<CampaignTag>();
    public DbSet<CampaignClosureRecord> CampaignClosureRecords => Set<CampaignClosureRecord>();
    public DbSet<CampaignExpense> CampaignExpenses => Set<CampaignExpense>();

    // Planning
    public DbSet<CalendarEntry> CalendarEntries => Set<CalendarEntry>();
    public DbSet<ResourceCapacity> ResourceCapacities => Set<ResourceCapacity>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SavedView> SavedViews => Set<SavedView>();

    // Content
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<ContentDependency> ContentDependencies => Set<ContentDependency>();
    public DbSet<Brief> Briefs => Set<Brief>();
    public DbSet<TaskChecklistItem> TaskChecklistItems => Set<TaskChecklistItem>();
    public DbSet<ContentTask> ContentTasks => Set<ContentTask>();
    public DbSet<Tag> Tags => Set<Tag>();

    // Assets
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetVersion> AssetVersions => Set<AssetVersion>();
    public DbSet<AssetPreview> AssetPreviews => Set<AssetPreview>();
    public DbSet<AssetRestoreRecord> AssetRestoreRecords => Set<AssetRestoreRecord>();
    public DbSet<AssetUploadSession> AssetUploadSessions => Set<AssetUploadSession>();

    // Approval
    public DbSet<ApprovalCycle> ApprovalCycles => Set<ApprovalCycle>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<ClientPortalSession> ClientPortalSessions => Set<ClientPortalSession>();
    public DbSet<PortalAccessToken> PortalAccessTokens => Set<PortalAccessToken>();
    public DbSet<ContentMessage> ContentMessages => Set<ContentMessage>();
    public DbSet<ContentAnnotation> ContentAnnotations => Set<ContentAnnotation>();

    // Publication
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<PublicationVariant> PublicationVariants => Set<PublicationVariant>();
    public DbSet<PublicationAttempt> PublicationAttempts => Set<PublicationAttempt>();
    public DbSet<SocialConnection> SocialConnections => Set<SocialConnection>();
    public DbSet<PublicationLog> PublicationLogs => Set<PublicationLog>();

    // Performance
    public DbSet<PerformanceSnapshot> PerformanceSnapshots => Set<PerformanceSnapshot>();
    public DbSet<CampaignPerformanceAggregate> CampaignPerformanceAggregates => Set<CampaignPerformanceAggregate>();
    public DbSet<ReportExport> ReportExports => Set<ReportExport>();

    // Missions
    public DbSet<Mission> Missions => Set<Mission>();
    public DbSet<MissionMember> MissionMembers => Set<MissionMember>();
    public DbSet<MissionMilestone> MissionMilestones => Set<MissionMilestone>();
    public DbSet<MissionDelivery> MissionDeliveries => Set<MissionDelivery>();
    public DbSet<MissionTask> MissionTasks => Set<MissionTask>();
    public DbSet<DeliveryNote> DeliveryNotes => Set<DeliveryNote>();
    public DbSet<BillingRecord> BillingRecords => Set<BillingRecord>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<EscalationRule> EscalationRules => Set<EscalationRule>();

    // Audit
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<IntegrationEvent> IntegrationEvents => Set<IntegrationEvent>();
    public DbSet<IntegrationFailure> IntegrationFailures => Set<IntegrationFailure>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // DomainEvent is NOT a database entity — ignore it from the EF model
        builder.Ignore<DomainEvent>();

        // Apply all configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(MaonoDbContext).Assembly);

        // Global Query Filters — Multi-tenant isolation
        ApplyTenantFilters(builder);

        // Global Query Filters — Soft delete
        ApplySoftDeleteFilters(builder);
    }

    private void ApplyTenantFilters(ModelBuilder builder)
    {
        // Apply WorkspaceId filter on all TenantEntity-derived types
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(MaonoDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { builder });
            }
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder builder) where TEntity : TenantEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => _currentWorkspaceId == null || e.WorkspaceId == _currentWorkspaceId);
    }

    private void ApplySoftDeleteFilters(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(MaonoDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { builder });
            }
        }
    }

    private void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : class, ISoftDeletable
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set audit fields
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
