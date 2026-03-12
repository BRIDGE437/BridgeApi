using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class PostLikeConfiguration : IEntityTypeConfiguration<PostLike>
{
    public void Configure(EntityTypeBuilder<PostLike> builder)
    {
        builder.HasKey(pl => pl.Id);
        builder.HasIndex(pl => new { pl.PostId, pl.UserId }).IsUnique();

        builder.HasOne(pl => pl.Post)
            .WithMany(p => p.PostLikes)
            .HasForeignKey(pl => pl.PostId);

        builder.HasOne(pl => pl.User)
            .WithMany(u => u.PostLikes)
            .HasForeignKey(pl => pl.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
