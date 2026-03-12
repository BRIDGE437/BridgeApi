namespace BridgeApi.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ConnectionId { get; set; }
    public string SenderId { get; set; } = null!;

    public string Content { get; set; } = null!;
    public bool IsRead { get; set; } = false;

    public Connection Connection { get; set; } = null!;
    public AppUser Sender { get; set; } = null!;
}
