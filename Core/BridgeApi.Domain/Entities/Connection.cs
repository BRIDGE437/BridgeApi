namespace BridgeApi.Domain.Entities;

public class Connection : BaseEntity
{
    public string SenderId { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;
    public Guid IntentId { get; set; }

    public string? Note { get; set; }

    // 0 = Pending, 1 = Accepted, 2 = Rejected
    public int Status { get; set; } = 0;

    public AppUser Sender { get; set; } = null!;
    public AppUser Receiver { get; set; } = null!;
    public Intent Intent { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
