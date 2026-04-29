namespace BridgeApi.Infrastructure.Services.Mailing;

internal sealed record EmailEnvelope(
    string ToAddress,
    string Subject,
    string HtmlBody,
    string PlainTextBody);
