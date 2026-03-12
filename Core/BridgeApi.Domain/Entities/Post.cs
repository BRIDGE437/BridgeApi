namespace BridgeApi.Domain.Entities;

public class Post : BaseEntity
{
    public string UserId { get; set; } = null!;

    public string? Content { get; set; }
    public string? ImageUrl { get; set; }

    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;

    public AppUser User { get; set; } = null!;
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
}
