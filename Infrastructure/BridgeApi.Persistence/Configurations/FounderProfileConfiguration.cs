using BridgeApi.Domain.Entities;
using BridgeApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class FounderProfileConfiguration : IEntityTypeConfiguration<FounderProfile>
{
    public void Configure(EntityTypeBuilder<FounderProfile> builder)
    {
        builder.Property(f => f.StartupName).IsRequired().HasMaxLength(150);
        builder.Property(f => f.StartupWebsite).HasMaxLength(500);
        builder.Property(f => f.PitchDeckUrl).HasMaxLength(500);
        builder.Property(f => f.OneLiner).HasMaxLength(160);
        builder.Property(f => f.ProblemStatement).HasMaxLength(500);
        builder.Property(f => f.Country).HasMaxLength(100);
        builder.Property(f => f.FundingNeedUsd).HasPrecision(18, 2);

        builder.Property(f => f.SecondarySectors)
            .HasConversion(
                v => v.Select(x => (int)x).ToArray(),
                v => v.Select(x => (Sector)x).ToArray(),
                new ValueComparer<Sector[]>(
                    (a, b) => (a ?? Array.Empty<Sector>()).SequenceEqual(b ?? Array.Empty<Sector>()),
                    v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                    v => v.ToArray()))
            .HasColumnType("integer[]");

        builder.HasIndex(f => f.UserProfileId).IsUnique();
        builder.HasIndex(f => f.PrimarySector);
        builder.HasIndex(f => f.Stage);
        builder.HasIndex(f => f.IsActivelyFundraising);
    }
}
