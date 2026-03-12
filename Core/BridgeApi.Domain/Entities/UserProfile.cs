namespace BridgeApi.Domain.Entities;

public class UserProfile : BaseEntity
{
    public string UserId { get; set; } = null!;

    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public string? ProfileImage { get; set; }

    public string? PhoneNumber { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    public AppUser User { get; set; } = null!;
}
