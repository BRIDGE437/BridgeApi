using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using Scriban;

namespace BridgeApi.Infrastructure.Services.Mailing;

internal sealed class ScribanEmailTemplateRenderer : IEmailTemplateRenderer
{
    private static readonly Assembly Assembly = typeof(ScribanEmailTemplateRenderer).Assembly;
    private static readonly ConcurrentDictionary<string, Template> Cache = new();
    private const string LayoutName = "_layout";

    public (string Html, string PlainText) Render(string templateName, object model)
    {
        var bodyTemplate = GetTemplate(templateName);
        var bodyHtml = bodyTemplate.Render(model);

        var layout = GetTemplate(LayoutName);
        var wrapped = layout.Render(new { Body = bodyHtml });

        var plainText = HtmlToPlainText(bodyHtml);
        return (wrapped, plainText);
    }

    private static Template GetTemplate(string name)
    {
        return Cache.GetOrAdd(name, key =>
        {
            var resourceName = Assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith($"{key}.html", StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Email template '{key}' not found as embedded resource.");

            using var stream = Assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();

            var template = Template.Parse(text);
            if (template.HasErrors)
                throw new InvalidOperationException($"Template '{key}' parse error: {template.Messages}");
            return template;
        });
    }

    private static string HtmlToPlainText(string html)
    {
        var noTags = Regex.Replace(html, "<[^>]+>", string.Empty);
        var decoded = System.Net.WebUtility.HtmlDecode(noTags);
        return Regex.Replace(decoded, @"\s+", " ").Trim();
    }
}
