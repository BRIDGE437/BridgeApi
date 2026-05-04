namespace MatchingApi.Helpers;

internal static class ModelHelpers
{
    internal static List<string> ParseCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return new List<string>();
        return value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();
    }

    internal static (string City, string Country) ParseHq(string? hq)
    {
        if (string.IsNullOrWhiteSpace(hq)) return ("", "");
        var parts = hq.Split('/', StringSplitOptions.TrimEntries);
        return parts.Length >= 2
            ? (parts[0].Trim(), parts[1].Trim())
            : (parts[0].Trim(), "");
    }
}
