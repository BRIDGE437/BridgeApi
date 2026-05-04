namespace BridgeApi.Infrastructure.Services.Mailing;

internal interface IEmailSender
{
    Task SendAsync(EmailEnvelope envelope, CancellationToken cancellationToken);
}
