namespace BridgeApi.Domain.Entities;

public class PostComment : BaseEntity
{
    public Guid PostId { get; set; }
    public string UserId { get; set; } = null!;

    public string CommentText { get; set; } = null!;

    public Post Post { get; set; } = null!;
    public AppUser User { get; set; } = null!;
}
