using Maono.Domain.Assets.Entities;
using Maono.Domain.Approval.Entities;
using Maono.Domain.Missions.Entities;
using Maono.Domain.Publications.Entities;
using Maono.Domain.Performance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maono.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.ContentItem)
            .WithMany(c => c.Assets)
            .HasForeignKey(a => a.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssetVersionConfiguration : IEntityTypeConfiguration<AssetVersion>
{
    public void Configure(EntityTypeBuilder<AssetVersion> builder)
    {
        builder.HasKey(v => v.Id);
        builder.HasIndex(v => new { v.AssetId, v.VersionNumber }).IsUnique();

        builder.HasOne(v => v.Asset)
            .WithMany(a => a.Versions)
            .HasForeignKey(v => v.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Preview)
            .WithOne(p => p.AssetVersion)
            .HasForeignKey<AssetPreview>(p => p.AssetVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApprovalCycleConfiguration : IEntityTypeConfiguration<ApprovalCycle>
{
    public void Configure(EntityTypeBuilder<ApprovalCycle> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.ContentItemId, a.RevisionRound });

        builder.HasOne(a => a.ContentItem)
            .WithMany()
            .HasForeignKey(a => a.ContentItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PublicationConfiguration : IEntityTypeConfiguration<Publication>
{
    public void Configure(EntityTypeBuilder<Publication> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.WorkspaceId, p.Status });
        builder.HasIndex(p => new { p.WorkspaceId, p.ScheduledAtUtc });

        builder.HasOne(p => p.ContentItem)
            .WithMany()
            .HasForeignKey(p => p.ContentItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SocialConnectionConfiguration : IEntityTypeConfiguration<SocialConnection>
{
    public void Configure(EntityTypeBuilder<SocialConnection> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => new { s.WorkspaceId, s.Platform, s.ExternalAccountId }).IsUnique();
    }
}

public class MissionConfiguration : IEntityTypeConfiguration<Mission>
{
    public void Configure(EntityTypeBuilder<Mission> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(300);
        builder.HasIndex(m => new { m.WorkspaceId, m.Status });
        builder.HasIndex(m => new { m.WorkspaceId, m.OwnerUserId });
        builder.Property(m => m.Budget).HasPrecision(18, 2);
    }
}

public class BillingRecordConfiguration : IEntityTypeConfiguration<BillingRecord>
{
    public void Configure(EntityTypeBuilder<BillingRecord> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Amount).HasPrecision(18, 2);
        builder.Property(b => b.Currency).HasMaxLength(3);

        builder.HasOne(b => b.Mission)
            .WithMany(m => m.BillingRecords)
            .HasForeignKey(b => b.MissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PerformanceSnapshotConfiguration : IEntityTypeConfiguration<PerformanceSnapshot>
{
    public void Configure(EntityTypeBuilder<PerformanceSnapshot> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.ContentItemId, p.CollectedAtUtc });
        builder.HasIndex(p => new { p.PublicationId, p.CollectedAtUtc });
        builder.Property(p => p.ConversionRate).HasPrecision(10, 4);
    }
}
