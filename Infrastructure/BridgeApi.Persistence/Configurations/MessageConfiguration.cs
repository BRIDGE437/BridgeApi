using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasOne(m => m.Connection)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConnectionId);

        builder.HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
