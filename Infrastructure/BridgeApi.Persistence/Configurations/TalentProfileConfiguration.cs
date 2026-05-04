using BridgeApi.Domain.Entities;
using BridgeApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class TalentProfileConfiguration : IEntityTypeConfiguration<TalentProfile>
{
    public void Configure(EntityTypeBuilder<TalentProfile> builder)
    {
        builder.Property(t => t.Headline).IsRequired().HasMaxLength(120);
        builder.Property(t => t.CurrentRole).HasMaxLength(150);
        builder.Property(t => t.CurrentCompany).HasMaxLength(150);
        builder.Property(t => t.ExpectedSalaryMonthlyUsd).HasPrecision(18, 2);

        builder.Property(t => t.Skills)
            .HasColumnType("text[]")
            .Metadata.SetValueComparer(new ValueComparer<string[]>(
                (a, b) => (a ?? Array.Empty<string>()).SequenceEqual(b ?? Array.Empty<string>()),
                v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                v => v.ToArray()));

        builder.Property(t => t.LookingFor)
            .HasConversion(
                v => v.Select(x => (int)x).ToArray(),
                v => v.Select(x => (EmploymentType)x).ToArray(),
                new ValueComparer<EmploymentType[]>(
                    (a, b) => (a ?? Array.Empty<EmploymentType>()).SequenceEqual(b ?? Array.Empty<EmploymentType>()),
                    v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                    v => v.ToArray()))
            .HasColumnType("integer[]");

        builder.Property(t => t.InterestedSectors)
            .HasConversion(
                v => v.Select(x => (int)x).ToArray(),
                v => v.Select(x => (Sector)x).ToArray(),
                new ValueComparer<Sector[]>(
                    (a, b) => (a ?? Array.Empty<Sector>()).SequenceEqual(b ?? Array.Empty<Sector>()),
                    v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x)),
                    v => v.ToArray()))
            .HasColumnType("integer[]");

        builder.HasIndex(t => t.UserProfileId).IsUnique();
        builder.HasIndex(t => t.OpenToWork);
    }
}
