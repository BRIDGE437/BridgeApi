using BridgeApi.Domain.Enums;

namespace BridgeApi.Domain.Entities;

public class Notification : BaseEntity
{
    public string UserId { get; set; } = null!;
    public string ActorId { get; set; } = null!;
    public NotificationType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Message { get; set; }
    public bool IsRead { get; set; }

    // Navigation Properties
    public AppUser User { get; set; } = null!;
    public AppUser Actor { get; set; } = null!;
}
