namespace BridgeApi.Application.Dtos;

public class PasswordResetSettings
{
    public int TokenLifetimeMinutes { get; set; } = 15;
    public string FrontendBaseUrl { get; set; } = "http://localhost:3000";
    public string ResetPath { get; set; } = "/reset-password";
}
