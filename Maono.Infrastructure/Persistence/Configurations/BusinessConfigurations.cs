using Maono.Domain.Campaigns.Entities;
using Maono.Domain.Clients.Entities;
using Maono.Domain.Content.Entities;
using Maono.Domain.Planning.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maono.Infrastructure.Persistence.Configurations;

public class ClientOrganizationConfiguration : IEntityTypeConfiguration<ClientOrganization>
{
    public void Configure(EntityTypeBuilder<ClientOrganization> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(300);
        builder.HasIndex(c => new { c.WorkspaceId, c.Name });

        builder.HasOne(c => c.BrandProfile)
            .WithOne(b => b.ClientOrganization)
            .HasForeignKey<BrandProfile>(b => b.ClientOrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(300);
        builder.HasIndex(c => new { c.WorkspaceId, c.Status });
        builder.HasIndex(c => new { c.WorkspaceId, c.ClientOrganizationId });
        builder.Property(c => c.BudgetPlanned).HasPrecision(18, 2);
        builder.Property(c => c.BudgetSpent).HasPrecision(18, 2);

        builder.HasOne(c => c.ClosureRecord)
            .WithOne(cr => cr.Campaign)
            .HasForeignKey<CampaignClosureRecord>(cr => cr.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CalendarEntryConfiguration : IEntityTypeConfiguration<CalendarEntry>
{
    public void Configure(EntityTypeBuilder<CalendarEntry> builder)
    {
        builder.HasKey(c => c.Id);
        builder.HasIndex(c => new { c.WorkspaceId, c.PublicationDate });
        builder.HasIndex(c => new { c.WorkspaceId, c.Platform });

        builder.HasOne(c => c.Campaign)
            .WithMany()
            .HasForeignKey(c => c.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ContentItemConfiguration : IEntityTypeConfiguration<ContentItem>
{
    public void Configure(EntityTypeBuilder<ContentItem> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(500);
        builder.HasIndex(c => new { c.WorkspaceId, c.Status });
        builder.HasIndex(c => new { c.WorkspaceId, c.Deadline });
        builder.HasIndex(c => new { c.WorkspaceId, c.CalendarEntryId });

        builder.HasOne(c => c.CalendarEntry)
            .WithMany(ce => ce.ContentItems)
            .HasForeignKey(c => c.CalendarEntryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ContentDependencyConfiguration : IEntityTypeConfiguration<ContentDependency>
{
    public void Configure(EntityTypeBuilder<ContentDependency> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.SourceContent)
            .WithMany()
            .HasForeignKey(d => d.SourceContentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.BlockingContent)
            .WithMany()
            .HasForeignKey(d => d.BlockingContentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(t => new { t.WorkspaceId, t.Name }).IsUnique();
    }
}
