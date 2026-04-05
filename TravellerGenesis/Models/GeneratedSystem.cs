using System.Collections.Generic;
using TravellerSystemGenerator;

namespace TravellerGenesis.Models
{
    /// <summary>
    /// Wraps a generated (or loaded) StarSystem with GUI-specific state.
    /// </summary>
    internal class GeneratedSystem
    {
        /// <summary>The live StarSystem object. Null when loaded from a snapshot.</summary>
        public StarSystem? System { get; set; }

        /// <summary>Serializable snapshot of the system (always populated).</summary>
        public SystemSnapshot Snapshot { get; set; } = new();

        /// <summary>Path to the saved JSON file. Null = not yet saved to disk.</summary>
        public string? FilePath { get; set; }

        /// <summary>True when the system has unsaved changes (names, etc.).</summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// User-assigned names keyed by designation.
        /// Key "system" holds the system-level display name.
        /// Other keys are world/star designations, e.g. "A III", "A II a".
        /// </summary>
        public Dictionary<string, string> Names { get; set; } = new();

        // ── Convenience helpers ────────────────────────────────────────

        /// <summary>Display name: Names["system"] if set, else Snapshot.SystemName, else Seed.</summary>
        public string DisplayName =>
            Names.TryGetValue("system", out var n) && !string.IsNullOrWhiteSpace(n) ? n
            : !string.IsNullOrWhiteSpace(Snapshot.SystemName) ? Snapshot.SystemName
            : $"Seed {Snapshot.Seed}";

        public int Seed => Snapshot.Seed;
        public int GasGiantCount => Snapshot.GasGiantCount;
        public int PlanetoidBeltCount => Snapshot.PlanetoidBeltCount;
        public int TerrestrialPlanetCount => Snapshot.TerrestrialPlanetCount;
        public string MainworldUWP => Snapshot.Mainworld?.UWP ?? "";
        public int StarCount => Snapshot.Stars.Count;
    }
}
