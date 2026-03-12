namespace BridgeApi.Domain.Entities;

public class PostLike : BaseEntity
{
    public Guid PostId { get; set; }
    public string UserId { get; set; } = null!;

    public Post Post { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
