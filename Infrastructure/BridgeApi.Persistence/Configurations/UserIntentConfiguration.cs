using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class UserIntentConfiguration : IEntityTypeConfiguration<UserIntent>
{
    public void Configure(EntityTypeBuilder<UserIntent> builder)
    {
        builder.HasKey(ui => ui.Id);
        builder.HasIndex(ui => new { ui.UserId, ui.IntentId }).IsUnique();

        builder.HasOne(ui => ui.User)
            .WithMany(u => u.UserIntents)
            .HasForeignKey(ui => ui.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ui => ui.Intent)
            .WithMany(i => i.UserIntents)
            .HasForeignKey(ui => ui.IntentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
