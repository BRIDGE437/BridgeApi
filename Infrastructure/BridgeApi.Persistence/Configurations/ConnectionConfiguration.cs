using BridgeApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BridgeApi.Persistence.Configurations;

public class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.HasOne(c => c.Sender)
            .WithMany(u => u.SentConnections)
            .HasForeignKey(c => c.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Receiver)
            .WithMany(u => u.ReceivedConnections)
            .HasForeignKey(c => c.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Intent)
            .WithMany(i => i.Connections)
            .HasForeignKey(c => c.IntentId);
    }
}
