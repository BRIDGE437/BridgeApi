using Microsoft.AspNetCore.Identity;

namespace BridgeApi.Domain.Entities;

public class AppUser : IdentityUser<string>
{
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? AuthProvider { get; set; }
    public string? ProviderKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public UserProfile? UserProfile { get; set; }
    public ICollection<UserIntent> UserIntents { get; set; } = new List<UserIntent>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Connection> SentConnections { get; set; } = new List<Connection>();
    public ICollection<Connection> ReceivedConnections { get; set; } = new List<Connection>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();
    public ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
}
