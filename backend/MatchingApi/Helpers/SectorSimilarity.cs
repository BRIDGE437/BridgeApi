namespace MatchingApi.Helpers;

/// <summary>
/// Maps sectors to their similar/related sectors with similarity weights.
/// Exact match = 1.0, Similar = 0.5, Loosely related = 0.25
/// </summary>
public static class SectorSimilarity
{
    private static readonly Dictionary<string, Dictionary<string, double>> SimilarityMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Fintech"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["SaaS"] = 0.5, ["Business intelligence"] = 0.5, ["Insurtech"] = 0.5,
            ["Blockchain"] = 0.4, ["Regtech"] = 0.4, ["E-commerce"] = 0.25,
        },
        ["Artificial intelligence"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Developer Tools"] = 0.5, ["Big data"] = 0.5, ["Business intelligence"] = 0.5,
            ["Image process"] = 0.4, ["Robotics"] = 0.4, ["SaaS"] = 0.3,
        },
        ["SaaS"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Fintech"] = 0.5, ["Artificial intelligence"] = 0.4, ["Developer Tools"] = 0.5,
            ["Business intelligence"] = 0.5, ["Cloud"] = 0.5, ["Hrtech"] = 0.3,
            ["Regtech"] = 0.3, ["Retailtech"] = 0.3,
        },
        ["Gaming"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Social media"] = 0.4, ["Social network"] = 0.4,
            ["E-commerce"] = 0.2,
        },
        ["Healthtech"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Well-being"] = 0.5, ["Insurtech"] = 0.3,
            ["Artificial intelligence"] = 0.3, ["Edutech"] = 0.2,
        },
        ["E-commerce"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["E-commerce enabler"] = 0.6, ["Retailtech"] = 0.5, ["Fintech"] = 0.3,
            ["Marketingtech"] = 0.4, ["Logistics"] = 0.3, ["SaaS"] = 0.25,
        },
        ["E-commerce enabler"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["E-commerce"] = 0.6, ["Retailtech"] = 0.5, ["SaaS"] = 0.3,
        },
        ["Cybersecurity"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Cloud"] = 0.4, ["Developer Tools"] = 0.3, ["SaaS"] = 0.3,
            ["Regtech"] = 0.3,
        },
        ["Developer Tools"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["SaaS"] = 0.5, ["Artificial intelligence"] = 0.5, ["Cloud"] = 0.4,
            ["Big data"] = 0.4,
        },
        ["Business intelligence"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Big data"] = 0.5, ["Artificial intelligence"] = 0.5, ["SaaS"] = 0.4,
            ["Fintech"] = 0.3,
        },
        ["Energy"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Climatetech"] = 0.5, ["Sustainability"] = 0.5, ["Smart city"] = 0.4,
            ["Internet of things"] = 0.3,
        },
        ["Climatetech"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Energy"] = 0.5, ["Sustainability"] = 0.6, ["Agritech"] = 0.4,
        },
        ["Sustainability"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Climatetech"] = 0.6, ["Energy"] = 0.5, ["Agritech"] = 0.4,
        },
        ["Blockchain"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Fintech"] = 0.4, ["Cybersecurity"] = 0.3, ["SaaS"] = 0.2,
        },
        ["Edutech"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Hrtech"] = 0.4, ["SaaS"] = 0.3, ["Artificial intelligence"] = 0.3,
        },
        ["Social media"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Social network"] = 0.6, ["Gaming"] = 0.4, ["Marketingtech"] = 0.4,
        },
        ["Social network"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Social media"] = 0.6, ["Gaming"] = 0.3,
        },
        ["Marketingtech"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Social media"] = 0.4, ["E-commerce"] = 0.4, ["SaaS"] = 0.3,
        },
        ["Logistics"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["E-commerce"] = 0.3, ["Mobility"] = 0.4, ["Smart city"] = 0.3,
        },
        ["Robotics"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Artificial intelligence"] = 0.4, ["Industry40"] = 0.5, ["Electronics"] = 0.4,
        },
        ["Agritech"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Climatetech"] = 0.4, ["Sustainability"] = 0.4, ["Internet of things"] = 0.3,
        },
        ["Insurtech"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Fintech"] = 0.5, ["Healthtech"] = 0.3,
        },
        ["Telecom"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Internet of things"] = 0.4, ["Cybersecurity"] = 0.3, ["Cloud"] = 0.3,
        },
        ["Retailtech"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["E-commerce"] = 0.5, ["E-commerce enabler"] = 0.5, ["Fintech"] = 0.3,
        },
    };

    /// <summary>
    /// Computes a similarity-aware sector score between a startup's tags
    /// and an investor's preferred sectors.
    ///
    /// Exact match contributes 1.0, similar sectors contribute their weight.
    /// Final score = best match per investor sector, averaged and scaled.
    /// </summary>
    public static double CalculateSectorScore(List<string> startupTags, List<string> investorSectors, double maxScore)
    {
        if (startupTags.Count == 0 || investorSectors.Count == 0) return 0;

        double totalScore = 0;

        foreach (var investorSector in investorSectors)
        {
            double bestMatch = 0;

            foreach (var tag in startupTags)
            {
                // Exact match = 1.0
                if (tag.Equals(investorSector, StringComparison.OrdinalIgnoreCase))
                {
                    bestMatch = 1.0;
                    break; // Can't do better than exact
                }

                // Similarity match
                double similarity = GetSimilarity(investorSector, tag);
                if (similarity > bestMatch)
                    bestMatch = similarity;
            }

            totalScore += bestMatch;
        }

        // Normalize: average score across investor sectors, scaled to maxScore
        double normalized = (totalScore / investorSectors.Count) * maxScore;
        return Math.Round(normalized, 1);
    }

    /// <summary>
    /// Returns similarity between two sectors (0.0 to 1.0).
    /// Checks both directions (A→B and B→A).
    /// </summary>
    public static double GetSimilarity(string sectorA, string sectorB)
    {
        if (sectorA.Equals(sectorB, StringComparison.OrdinalIgnoreCase))
            return 1.0;

        // Check A → B
        if (SimilarityMap.TryGetValue(sectorA, out var mapA) &&
            mapA.TryGetValue(sectorB, out var simAB))
            return simAB;

        // Check B → A (bidirectional)
        if (SimilarityMap.TryGetValue(sectorB, out var mapB) &&
            mapB.TryGetValue(sectorA, out var simBA))
            return simBA;

        return 0.0;
    }
}
