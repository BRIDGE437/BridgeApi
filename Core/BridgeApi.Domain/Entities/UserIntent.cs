namespace BridgeApi.Domain.Entities;

/// <summary>
/// Kullanıcının sahip olduğu niyet/rozet (örn. Actively Hiring, Raising, Idea Stage).
/// Intent ile User arasında çoktan çoğa ilişki.
/// </summary>
public class UserIntent : BaseEntity
{
    public string UserId { get; set; } = null!;
    public Guid IntentId { get; set; }

    public AppUser User { get; set; } = null!;
    public Intent Intent { get; set; } = null!;
}
