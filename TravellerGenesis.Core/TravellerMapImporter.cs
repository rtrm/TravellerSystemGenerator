using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    internal record SectorInfo(string Abbreviation, string DisplayName);

    internal record T5SystemData(
        string Hex,
        string Name,
        string UWP,
        string Remarks,
        int Importance,
        string EconEx,
        string CulturalEx,
        string Nobility,
        string Bases,
        string TravelZone,
        string PBG,
        int WorldCount,
        string Allegiance,
        string Stars
    );

    internal static class TravellerMapImporter
    {
        private static readonly HttpClient Http = new HttpClient
        {
            DefaultRequestHeaders = { { "User-Agent", "TravellerGenesis/2.0" } }
        };

        private const string BaseUrl = "https://travellermap.com";

        // ── Sector prefetch cache ─────────────────────────────────────

        private static Task<List<SectorInfo>>? _sectorFetchTask;

        /// <summary>Start the sector list fetch in the background (call on app startup).</summary>
        internal static void BeginPrefetchSectors()
        {
            _sectorFetchTask ??= FetchSectorsInternalAsync();
        }

        // ── Sector list ───────────────────────────────────────────────

        internal static Task<List<SectorInfo>> GetSectorsAsync()
        {
            return _sectorFetchTask ??= FetchSectorsInternalAsync();
        }

        private static async Task<List<SectorInfo>> FetchSectorsInternalAsync()
        {
            string json = await Http.GetStringAsync($"{BaseUrl}/data");

            using var doc = JsonDocument.Parse(json);
            var sectors = new List<SectorInfo>();

            if (!doc.RootElement.TryGetProperty("Sectors", out var sectorArray))
                return sectors;

            foreach (var sector in sectorArray.EnumerateArray())
            {
                // Filter: M1105 only and Tags must contain "OTU"
                if (!sector.TryGetProperty("Milieu", out var milieuEl) ||
                    milieuEl.GetString() != "M1105")
                    continue;

                if (!sector.TryGetProperty("Tags", out var tagsEl))
                    continue;
                string tags = tagsEl.GetString() ?? "";
                if (!tags.Contains("OTU"))
                    continue;

                if (!sector.TryGetProperty("Abbreviation", out var abbrevEl))
                    continue;
                string abbrev = abbrevEl.GetString() ?? "";
                if (string.IsNullOrWhiteSpace(abbrev)) continue;

                // Pick display name: first entry without a "Lang" field, else first entry
                string displayName = abbrev;
                if (sector.TryGetProperty("Names", out var namesArray))
                {
                    string? firstAny = null;
                    string? firstEnglish = null;
                    foreach (var nameEl in namesArray.EnumerateArray())
                    {
                        string? text = nameEl.TryGetProperty("Text", out var t) ? t.GetString() : null;
                        if (text == null) continue;
                        firstAny ??= text;
                        if (firstEnglish == null && !nameEl.TryGetProperty("Lang", out _))
                            firstEnglish = text;
                    }
                    displayName = firstEnglish ?? firstAny ?? abbrev;
                }

                sectors.Add(new SectorInfo(abbrev, displayName));
            }

            return sectors.OrderBy(s => s.DisplayName).ToList();
        }

        // ── Sector raw text (for subsector name parsing) ──────────────

        internal static async Task<string> GetSectorTextAsync(string abbrev)
        {
            return await Http.GetStringAsync($"{BaseUrl}/data/{Uri.EscapeDataString(abbrev)}");
        }

        // ── Parse subsector names from sector raw text ────────────────

        // Returns A→"Cronor", B→"Jewell", etc.
        internal static Dictionary<char, string> ParseSubsectorNames(string text)
        {
            var result = new Dictionary<char, string>();
            // Lines like: # Subsector A: Cronor
            var rx = new Regex(@"^#\s+Subsector\s+([A-P]):\s+(.+)$", RegexOptions.Multiline);
            foreach (Match m in rx.Matches(text))
            {
                char letter = m.Groups[1].Value[0];
                string name = m.Groups[2].Value.Trim();
                result[letter] = name;
            }
            return result;
        }

        // ── Fetch and parse system list ───────────────────────────────

        internal static async Task<List<T5SystemData>> GetSystemsAsync(
            string abbrev, char? subsectorLetter = null)
        {
            string url = subsectorLetter.HasValue
                ? $"{BaseUrl}/api/sec?sector={Uri.EscapeDataString(abbrev)}&subsector={subsectorLetter.Value}&type=TabDelimited"
                : $"{BaseUrl}/api/sec?sector={Uri.EscapeDataString(abbrev)}&type=TabDelimited";

            string text = await Http.GetStringAsync(url);
            return ParseTabDelimited(text);
        }

        // ── Tab-delimited parser ──────────────────────────────────────

        internal static List<T5SystemData> ParseTabDelimited(string text)
        {
            var result = new List<T5SystemData>();
            string[]? headers = null;

            foreach (string rawLine in text.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r');

                // Skip comments and blank lines
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;

                string[] cols = line.Split('\t');

                // First non-comment non-blank line is the header
                if (headers == null)
                {
                    headers = cols;
                    continue;
                }

                // Map column names to indices once
                var idx = BuildIndexMap(headers);

                string hex        = Get(cols, idx, "Hex");
                string name       = Get(cols, idx, "Name");
                string uwp        = Get(cols, idx, "UWP");
                string remarks    = Get(cols, idx, "Remarks");
                string ixRaw      = Get(cols, idx, "{Ix}");
                string econEx     = Get(cols, idx, "(Ex)");
                string culturalEx = Get(cols, idx, "[Cx]");
                string nobility   = Get(cols, idx, "Nobility");
                string bases      = Get(cols, idx, "Bases");
                string zone       = Get(cols, idx, "Zone");
                string pbg        = Get(cols, idx, "PBG");
                string wRaw       = Get(cols, idx, "W");
                string allegiance = Get(cols, idx, "Allegiance");
                // Header can be "Stars" or "Stellar" depending on API version
                string stars      = Get(cols, idx, "Stars");
                if (string.IsNullOrEmpty(stars)) stars = Get(cols, idx, "Stellar");

                // Parse importance: strip braces, trim spaces
                int importance = 0;
                string ixClean = ixRaw.Trim('{', '}', ' ');
                int.TryParse(ixClean, out importance);

                // Parse world count
                int worldCount = 0;
                int.TryParse(wRaw, out worldCount);

                // Normalise zone: "-" or empty → null stored as ""
                if (zone == "-") zone = "";

                if (string.IsNullOrWhiteSpace(hex) || string.IsNullOrWhiteSpace(uwp))
                    continue;

                result.Add(new T5SystemData(
                    hex, name, uwp, remarks,
                    importance, econEx, culturalEx,
                    nobility, bases, zone,
                    pbg, worldCount, allegiance, stars
                ));
            }

            return result;
        }

        // ── Helpers ───────────────────────────────────────────────────

        private static Dictionary<string, int> BuildIndexMap(string[] headers)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
                map[headers[i].Trim()] = i;
            return map;
        }

        private static string Get(string[] cols, Dictionary<string, int> idx, string key)
        {
            if (!idx.TryGetValue(key, out int i)) return "";
            if (i >= cols.Length) return "";
            return cols[i].Trim();
        }

        // eHex decode: 0-9 → 0-9, A-H → 10-17, J-N → 18-22, P-Z → 23-33
        internal static int EhexDecode(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            c = char.ToUpperInvariant(c);
            if (c >= 'A' && c <= 'H') return c - 'A' + 10;
            if (c >= 'J' && c <= 'N') return c - 'J' + 18;  // skip I
            if (c >= 'P' && c <= 'Z') return c - 'P' + 23;  // skip O
            return 0;
        }
    }
}
