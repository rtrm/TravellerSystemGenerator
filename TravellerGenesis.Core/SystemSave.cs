using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TravellerSystemGenerator
{
    internal class SystemSnapshot
    {
        public string Version { get; set; } = "";
        public int Seed { get; set; }
        public string? SystemName { get; set; }
        public string GeneratedAt { get; set; } = "";
        public List<StarDisplayData> Stars { get; set; } = new();
        public List<WorldDisplayData> Worlds { get; set; } = new();
        public List<SurveyData> Surveys { get; set; } = new();
        public MainworldData? Mainworld { get; set; }
        public List<AdditionalInhabitedWorld> AdditionalInhabitedWorlds { get; set; } = new();
        public List<Faction> WorldFactions { get; set; } = new();
        public List<FactionRelationship> FactionRelationships { get; set; } = new();
        public int GasGiantCount { get; set; }
        public int PlanetoidBeltCount { get; set; }
        public int TerrestrialPlanetCount { get; set; }
        // User-assigned names: key = designation ("A III", "A II a") or "system"
        public Dictionary<string, string> Names { get; set; } = new();
    }

    internal static class SystemSave
    {
        private const string JsonFolder = "systems";

        private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions ReadOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        internal static void Save(
            StarSystem starSystem,
            List<StarDisplayData> stars,
            List<WorldDisplayData> worlds,
            List<SurveyData> surveys,
            List<AdditionalInhabitedWorld> aiws,
            List<Faction> factions,
            List<FactionRelationship> relationships,
            bool uniqueFilename)
        {
            var snapshot = new SystemSnapshot
            {
                Version         = TravellerSystemGenerator.Version.VersionString,
                Seed            = starSystem.Seed,
                SystemName      = starSystem.systemName,
                GeneratedAt     = DateTime.UtcNow.ToString("o"),
                Stars           = stars,
                Worlds          = worlds,
                Surveys         = surveys,
                Mainworld       = starSystem.mainworld,
                AdditionalInhabitedWorlds = aiws,
                WorldFactions   = factions,
                FactionRelationships = relationships,
                GasGiantCount        = starSystem.GasGiantCount,
                PlanetoidBeltCount   = starSystem.PlanetoidBeltCount,
                TerrestrialPlanetCount = starSystem.TerrestrialPlanetCount
            };

            try
            {
                Directory.CreateDirectory(JsonFolder);
                string filename = uniqueFilename
                    ? $"system_{starSystem.Seed}.json"
                    : "StarSystem.json";
                string path = Path.Combine(JsonFolder, filename);
                File.WriteAllText(path, JsonSerializer.Serialize(snapshot, WriteOptions));
                Console.WriteLine($"System saved to: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARNING: Could not save JSON snapshot - {ex.Message}");
            }
        }

        internal static SystemSnapshot Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Snapshot file not found: {filePath}");

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<SystemSnapshot>(json, ReadOptions)
                ?? throw new InvalidDataException($"Failed to deserialize snapshot: {filePath}");
        }
    }
}
