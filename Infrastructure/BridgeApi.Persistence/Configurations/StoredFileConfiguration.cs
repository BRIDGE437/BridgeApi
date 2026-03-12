using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.Property(f => f.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(f => f.StoredFileName).HasMaxLength(255).IsRequired();
        builder.Property(f => f.Path).HasMaxLength(500).IsRequired();
        builder.Property(f => f.Url).HasMaxLength(1000).IsRequired();
        builder.Property(f => f.Storage).HasMaxLength(50).IsRequired();
        builder.Property(f => f.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(f => f.UploadedByUserId).IsRequired();

        builder.HasOne(f => f.UploadedByUser)
            .WithMany()
            .HasForeignKey(f => f.UploadedByUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.UploadedByUserId);
        builder.HasIndex(f => f.Category);
    }
}
