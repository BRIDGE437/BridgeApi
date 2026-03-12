namespace BridgeApi.Infrastructure.Configuration;

public class JwtSettings
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SecurityKey { get; set; } = null!;
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
