using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        // AppUser - UserProfile (1-1)
        builder.HasOne(u => u.UserProfile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);
    }
}
