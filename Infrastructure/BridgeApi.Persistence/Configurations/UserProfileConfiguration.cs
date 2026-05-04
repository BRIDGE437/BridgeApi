using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(100);
        builder.Property(p => p.Surname).HasMaxLength(100);
        builder.Property(p => p.Title).HasMaxLength(150);
        builder.Property(p => p.Headline).HasMaxLength(200);
        builder.Property(p => p.Bio).HasMaxLength(500);
        builder.Property(p => p.Location).HasMaxLength(150);
        builder.Property(p => p.ProfileImage).HasMaxLength(500);
        builder.Property(p => p.CoverImage).HasMaxLength(500);
        builder.Property(p => p.PhoneNumber).HasMaxLength(40);
        builder.Property(p => p.LinkedInUrl).HasMaxLength(500);
        builder.Property(p => p.GitHubUrl).HasMaxLength(500);
        builder.Property(p => p.WebsiteUrl).HasMaxLength(500);

        builder.HasOne(p => p.FounderProfile)
            .WithOne(f => f.UserProfile)
            .HasForeignKey<FounderProfile>(f => f.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.InvestorProfile)
            .WithOne(i => i.UserProfile)
            .HasForeignKey<InvestorProfile>(i => i.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.TalentProfile)
            .WithOne(t => t.UserProfile)
            .HasForeignKey<TalentProfile>(t => t.UserProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
