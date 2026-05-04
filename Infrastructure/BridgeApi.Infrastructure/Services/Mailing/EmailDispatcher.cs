using System.Threading.Channels;
using BridgeApi.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BridgeApi.Infrastructure.Services.Mailing;

internal sealed class EmailDispatcher : IEmailService
{
    private readonly Channel<EmailEnvelope> _channel;
    private readonly IEmailTemplateRenderer _renderer;
    private readonly EmailOptions _options;
    private readonly ILogger<EmailDispatcher> _logger;

    public EmailDispatcher(
        Channel<EmailEnvelope> channel,
        IEmailTemplateRenderer renderer,
        IOptions<EmailOptions> options,
        ILogger<EmailDispatcher> logger)
    {
        _channel = channel;
        _renderer = renderer;
        _options = options.Value;
        _logger = logger;
    }

    public Task SendPasswordResetAsync(string toEmail, string userName, string resetUrl, int expiresInMinutes, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            UserName = userName,
            ResetUrl = resetUrl,
            ExpiresInMinutes = expiresInMinutes,
            BrandName = _options.FromName
        };
        return SendAsync(toEmail, "BRIDGE - Şifre Sıfırlama", "password-reset", model, cancellationToken);
    }

    public Task SendPasswordChangedAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
    {
        var model = new
        {
            UserName = userName,
            BrandName = _options.FromName,
            SupportUrl = _options.FrontendBaseUrl
        };
        return SendAsync(toEmail, "BRIDGE - Şifreniz değiştirildi", "password-changed", model, cancellationToken);
    }

    public async Task SendAsync(string toEmail, string subject, string templateName, object model, CancellationToken cancellationToken = default)
    {
        var (html, text) = _renderer.Render(templateName, model);
        var envelope = new EmailEnvelope(toEmail, subject, html, text);

        if (!_channel.Writer.TryWrite(envelope))
        {
            await _channel.Writer.WriteAsync(envelope, cancellationToken);
        }

        _logger.LogDebug("Queued email template {Template}", templateName);
    }
}
