namespace MatchingApi.Helpers;

public static class RegionMapper
{
    // ── Country → Region mapping ──
    private static readonly Dictionary<string, string> CountryToRegion = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Turkey"] = "Turkey",
        ["UK"] = "Europe", ["United Kingdom"] = "Europe",
        ["Germany"] = "Europe", ["France"] = "Europe",
        ["Netherlands"] = "Europe", ["Spain"] = "Europe",
        ["Italy"] = "Europe", ["Sweden"] = "Europe",
        ["Switzerland"] = "Europe", ["Ireland"] = "Europe",
        ["Finland"] = "Europe", ["Norway"] = "Europe",
        ["Denmark"] = "Europe", ["Belgium"] = "Europe",
        ["Austria"] = "Europe", ["Poland"] = "Europe",
        ["Portugal"] = "Europe", ["Greece"] = "Europe",
        ["Czech Republic"] = "Europe", ["Romania"] = "Europe",
        ["USA"] = "USA", ["United States"] = "USA",
        ["Delaware"] = "USA", ["California"] = "USA",
        ["New York"] = "USA", ["Texas"] = "USA",
        ["Massachusetts"] = "USA", ["Washington"] = "USA",
        ["Florida"] = "USA", ["Illinois"] = "USA",
        ["Georgia"] = "USA", ["Colorado"] = "USA",
        ["UAE"] = "MENA", ["Qatar"] = "MENA",
        ["Saudi Arabia"] = "MENA", ["Bahrain"] = "MENA",
        ["Kuwait"] = "MENA", ["Oman"] = "MENA",
        ["Egypt"] = "MENA", ["Jordan"] = "MENA",
        ["Lebanon"] = "MENA", ["Morocco"] = "MENA",
        ["Tunisia"] = "MENA", ["Iraq"] = "MENA",
        ["India"] = "Asia", ["Singapore"] = "Asia",
        ["Japan"] = "Asia", ["South Korea"] = "Asia",
        ["China"] = "Asia", ["Indonesia"] = "Asia",
        ["Canada"] = "North America",
        ["Brazil"] = "LATAM", ["Mexico"] = "LATAM",
        ["Argentina"] = "LATAM", ["Colombia"] = "LATAM",
    };

    // ── City proximity: nearby cities with similarity weight (0.0–1.0) ──
    private static readonly Dictionary<string, Dictionary<string, double>> CityProximity = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Istanbul"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Bursa"] = 0.7, ["Kocaeli"] = 0.7, ["Tekirdağ"] = 0.7,
            ["Sakarya"] = 0.7, ["Edirne"] = 0.5, ["Yalova"] = 0.7,
            ["Bilecik"] = 0.5, ["Eskişehir"] = 0.4, ["Ankara"] = 0.3,
            ["Bolu"] = 0.5, ["Çanakkale"] = 0.5, ["Balıkesir"] = 0.4,
            ["Kırklareli"] = 0.6,
        },
        ["Ankara"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Eskişehir"] = 0.7, ["Konya"] = 0.5, ["Kırıkkale"] = 0.7,
            ["Bolu"] = 0.5, ["Çankırı"] = 0.7, ["Kırşehir"] = 0.6,
            ["Aksaray"] = 0.5, ["Istanbul"] = 0.3, ["Sakarya"] = 0.4,
            ["Kastamonu"] = 0.4, ["Yozgat"] = 0.5, ["Nevşehir"] = 0.5,
        },
        ["Izmir"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Manisa"] = 0.7, ["Aydın"] = 0.7, ["Denizli"] = 0.5,
            ["Muğla"] = 0.5, ["Balıkesir"] = 0.5, ["Uşak"] = 0.5,
            ["Kütahya"] = 0.4, ["Afyon"] = 0.4, ["Çanakkale"] = 0.4,
        },
        ["Antalya"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Burdur"] = 0.7, ["Isparta"] = 0.6, ["Mersin"] = 0.4,
            ["Muğla"] = 0.5, ["Konya"] = 0.4, ["Denizli"] = 0.4,
            ["Karaman"] = 0.5,
        },
        ["Bursa"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Istanbul"] = 0.7, ["Kocaeli"] = 0.6, ["Bilecik"] = 0.7,
            ["Eskişehir"] = 0.5, ["Balıkesir"] = 0.5, ["Yalova"] = 0.7,
            ["Kütahya"] = 0.5, ["Sakarya"] = 0.5,
        },
        ["Konya"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Ankara"] = 0.5, ["Aksaray"] = 0.7, ["Karaman"] = 0.7,
            ["Isparta"] = 0.5, ["Antalya"] = 0.4, ["Afyon"] = 0.5,
            ["Nevşehir"] = 0.5, ["Mersin"] = 0.4,
        },
        ["Adana"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Mersin"] = 0.7, ["Hatay"] = 0.6, ["Osmaniye"] = 0.7,
            ["Gaziantep"] = 0.5, ["Kayseri"] = 0.4, ["Kahramanmaraş"] = 0.5,
        },
        ["Gaziantep"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Adana"] = 0.5, ["Kahramanmaraş"] = 0.6, ["Şanlıurfa"] = 0.6,
            ["Kilis"] = 0.7, ["Hatay"] = 0.5, ["Adıyaman"] = 0.5,
            ["Malatya"] = 0.4,
        },
        ["Trabzon"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Rize"] = 0.7, ["Giresun"] = 0.7, ["Artvin"] = 0.6,
            ["Gümüşhane"] = 0.6, ["Ordu"] = 0.5, ["Bayburt"] = 0.5,
        },
        ["Rize"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Trabzon"] = 0.7, ["Artvin"] = 0.7, ["Giresun"] = 0.5,
            ["Gümüşhane"] = 0.5,
        },
        ["Kayseri"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Nevşehir"] = 0.7, ["Sivas"] = 0.5, ["Yozgat"] = 0.6,
            ["Kırşehir"] = 0.5, ["Niğde"] = 0.6, ["Aksaray"] = 0.5,
        },
        ["Eskişehir"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Ankara"] = 0.7, ["Bursa"] = 0.5, ["Kütahya"] = 0.7,
            ["Bilecik"] = 0.7, ["Afyon"] = 0.5, ["Istanbul"] = 0.4,
        },
        ["Samsun"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Ordu"] = 0.6, ["Amasya"] = 0.7, ["Tokat"] = 0.5,
            ["Sinop"] = 0.6, ["Çorum"] = 0.5,
        },
        ["Diyarbakır"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Batman"] = 0.7, ["Mardin"] = 0.6, ["Şanlıurfa"] = 0.5,
            ["Elazığ"] = 0.5, ["Bingöl"] = 0.5, ["Siirt"] = 0.5,
        },
        ["London"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Berlin"] = 0.3, ["Paris"] = 0.4, ["Amsterdam"] = 0.4,
        },
        ["Berlin"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["London"] = 0.3, ["Amsterdam"] = 0.4,
        },
        ["San Francisco"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["New York"] = 0.3, ["Boston"] = 0.3,
        },
        ["New York"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Boston"] = 0.5, ["San Francisco"] = 0.3,
        },
        ["Dubai"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Doha"] = 0.5, ["Riyadh"] = 0.4,
        },
        ["Doha"] = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Dubai"] = 0.5, ["Riyadh"] = 0.4,
        },
    };

    public static string GetRegion(string country)
    {
        if (string.IsNullOrWhiteSpace(country)) return "Other";
        return CountryToRegion.TryGetValue(country.Trim(), out var region) ? region : "Other";
    }

    /// <summary>
    /// Geo scoring with proximity:
    /// Same city → maxScore, Nearby city → maxScore × proximity,
    /// Same country → maxScore × 0.4, Same region → maxScore × 0.25
    /// </summary>
    public static double CalculateGeoScore(
        string city, string country,
        List<string> preferredCities, List<string> preferredRegions,
        double maxScore = 35.0)
    {
        double bestScore = 0;

        // 1. Exact city match
        if (!string.IsNullOrWhiteSpace(city) &&
            preferredCities.Any(pc => pc.Equals(city, StringComparison.OrdinalIgnoreCase)))
            return maxScore;

        // 2. Nearby city match
        if (!string.IsNullOrWhiteSpace(city))
        {
            foreach (var prefCity in preferredCities)
            {
                double prox = GetCityProximity(city, prefCity);
                if (prox > 0)
                {
                    double s = maxScore * prox;
                    if (s > bestScore) bestScore = s;
                }
            }
        }

        // 3. Country match
        if (!string.IsNullOrWhiteSpace(country) &&
            preferredRegions.Any(pr => pr.Equals(country, StringComparison.OrdinalIgnoreCase)))
        {
            double s = maxScore * 0.4;
            if (s > bestScore) bestScore = s;
        }

        // 4. Region match
        var region = GetRegion(country);
        if (preferredRegions.Any(pr => pr.Equals(region, StringComparison.OrdinalIgnoreCase)))
        {
            double s = maxScore * 0.25;
            if (s > bestScore) bestScore = s;
        }

        return Math.Round(bestScore, 1);
    }

    public static double GetCityProximity(string cityA, string cityB)
    {
        if (cityA.Equals(cityB, StringComparison.OrdinalIgnoreCase)) return 1.0;

        if (CityProximity.TryGetValue(cityA, out var mapA) &&
            mapA.TryGetValue(cityB, out var proxAB))
            return proxAB;

        if (CityProximity.TryGetValue(cityB, out var mapB) &&
            mapB.TryGetValue(cityA, out var proxBA))
            return proxBA;

        return 0.0;
    }
}
