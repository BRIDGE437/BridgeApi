namespace BridgeApi.Infrastructure.Services.Mailing;

internal interface IEmailTemplateRenderer
{
    (string Html, string PlainText) Render(string templateName, object model);
}
