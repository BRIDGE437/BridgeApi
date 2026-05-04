using BridgeApi.Domain.Entities;
using BridgeApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class InvestorProfileConfiguration : IEntityTypeConfiguration<InvestorProfile>
{
    public void Configure(EntityTypeBuilder<InvestorProfile> builder)
    {
        builder.Property(i => i.FirmName).HasMaxLength(150);
        builder.Property(i => i.FirmWebsite).HasMaxLength(500);
        builder.Property(i => i.InvestmentThesis).HasMaxLength(1000);
        builder.Property(i => i.CheckSizeMinUsd).HasPrecision(18, 2);
        builder.Property(i => i.CheckSizeMaxUsd).HasPrecision(18, 2);

        builder.Property(i => i.PreferredStages)
            .HasConversion(
                v => v.Select(x => (int)x).ToArray(),
                v => v.Select(x => (InvestmentStage)x).ToArray(),
                new ValueComparer<InvestmentStage[]>(
                    (a, b) => (a ?? Array.Empty<InvestmentStage>()).SequenceEqual(b ?? Array.Empty<InvestmentStage>()),
                    v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                    v => v.ToArray()))
            .HasColumnType("integer[]");

        builder.Property(i => i.PreferredSectors)
            .HasConversion(
                v => v.Select(x => (int)x).ToArray(),
                v => v.Select(x => (Sector)x).ToArray(),
                new ValueComparer<Sector[]>(
                    (a, b) => (a ?? Array.Empty<Sector>()).SequenceEqual(b ?? Array.Empty<Sector>()),
                    v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                    v => v.ToArray()))
            .HasColumnType("integer[]");

        builder.Property(i => i.PreferredGeographies)
            .HasColumnType("text[]")
            .Metadata.SetValueComparer(new ValueComparer<string[]>(
                (a, b) => (a ?? Array.Empty<string>()).SequenceEqual(b ?? Array.Empty<string>()),
                v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                v => v.ToArray()));

        builder.HasIndex(i => i.UserProfileId).IsUnique();
        builder.HasIndex(i => i.IsAcceptingPitches);
    }
}
