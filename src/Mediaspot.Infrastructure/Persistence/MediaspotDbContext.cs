using Mediaspot.Domain.Assets;
using Mediaspot.Domain.Assets.ValueObjects;
using Mediaspot.Domain.Titles;
using Mediaspot.Domain.Transcoding;
using Microsoft.EntityFrameworkCore;

namespace Mediaspot.Infrastructure.Persistence;

public sealed class MediaspotDbContext(DbContextOptions<MediaspotDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<TranscodeJob> TranscodeJobs => Set<TranscodeJob>();
    public DbSet<Title> Titles => Set<Title>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.ExternalId).IsRequired();
            b.OwnsOne(a => a.Metadata, m =>
            {
                m.Property(x => x.Title).IsRequired();
                m.Property(x => x.Description);
                m.Property(x => x.Language);
            });

            b.Ignore(a => a.MediaFiles);

            b.OwnsMany<MediaFile>("_mediaFiles", mf =>
            {
                mf.WithOwner().HasForeignKey("AssetId");
                mf.ToTable("MediaFiles");

                mf.HasKey(mf => mf.Id);

                mf.Property(x => x.Id)
                    .HasConversion(v => v.Value, v => new MediaFileId(v))
                    .HasColumnName("MediaFileId")
                    .IsRequired();

                mf.Property(mf => mf.Path).HasConversion(v => v.Value, v => new FilePath(v)).HasColumnName("Path").IsRequired();

                mf.Property(mf => mf.Duration).HasConversion(v => v.Value.TotalSeconds, v => new Duration(TimeSpan.FromMilliseconds(v))).HasColumnName("Duration").IsRequired();
            });

            b.Navigation("_mediaFiles").UsePropertyAccessMode(PropertyAccessMode.Field);

            b.Property<bool>("Archived").HasField("<Archived>k__BackingField");
            b.HasIndex(a => a.ExternalId).IsUnique();
        });

        modelBuilder.Entity<TranscodeJob>(b =>
        {
            b.HasKey(j => j.Id);
            b.Property(j => j.Preset).IsRequired();
            b.Property(j => j.Status);
            b.HasIndex(j => new { j.AssetId, j.Status });
        });

        modelBuilder.Entity<Title>(b =>
        {
            b.HasKey(t => t.Id);

            b.Property(t => t.Name)
                .IsRequired();

            b.Property(t => t.Description);

            b.Property(t => t.ReleaseDate);

            b.Property(t => t.Type)
                .IsRequired();

            // Enforces title-name uniqueness at the database level.
            b.HasIndex(t => t.Name)
                .IsUnique();
        });
    }
}
