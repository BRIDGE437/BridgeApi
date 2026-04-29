namespace BridgeApi.Application.Abstractions.Services;

public interface IEmailService
{
    Task SendPasswordResetAsync(string toEmail, string userName, string resetUrl, int expiresInMinutes, CancellationToken cancellationToken = default);

    Task SendPasswordChangedAsync(string toEmail, string userName, CancellationToken cancellationToken = default);

    Task SendAsync(string toEmail, string subject, string templateName, object model, CancellationToken cancellationToken = default);
}
