using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BridgeApi.Infrastructure.Services.Mailing;

internal sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailEnvelope envelope, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(envelope.ToAddress));
        message.Subject = envelope.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = envelope.HtmlBody,
            TextBody = envelope.PlainTextBody
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        var socketOptions = _options.Smtp.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(_options.Smtp.Host, _options.Smtp.Port, socketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.Smtp.Username))
        {
            await client.AuthenticateAsync(_options.Smtp.Username, _options.Smtp.Password ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent to {ToHash} subject {Subject}", Hash(envelope.ToAddress), envelope.Subject);
    }

    private static string Hash(string value)
    {
        // Avoid logging raw email addresses (PII). Log stable short hash instead.
        var bytes = System.Text.Encoding.UTF8.GetBytes(value);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash, 0, 6);
    }
}
