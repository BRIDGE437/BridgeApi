namespace BridgeApi.Infrastructure.Services.Mailing;

public class EmailOptions
{
    public SmtpOptions Smtp { get; set; } = new();
    public string FromAddress { get; set; } = "no-reply@bridge.local";
    public string FromName { get; set; } = "BRIDGE";
    public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
    public int PasswordResetTokenLifetimeMinutes { get; set; } = 15;
}

public class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public bool UseStartTls { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
