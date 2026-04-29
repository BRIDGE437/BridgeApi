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
}
