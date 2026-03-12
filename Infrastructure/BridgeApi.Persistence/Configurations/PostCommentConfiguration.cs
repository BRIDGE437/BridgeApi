using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class PostCommentConfiguration : IEntityTypeConfiguration<PostComment>
{
    public void Configure(EntityTypeBuilder<PostComment> builder)
    {
        builder.HasOne(pc => pc.Post)
            .WithMany(p => p.PostComments)
            .HasForeignKey(pc => pc.PostId);

        builder.HasOne(pc => pc.User)
            .WithMany(u => u.PostComments)
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
