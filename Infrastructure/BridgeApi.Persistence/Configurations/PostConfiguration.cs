using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId);
    }
}
