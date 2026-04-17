using System;
using System.IO;
using System.Text.Json;

namespace TravellerGenesis
{
    internal class AppSettings
    {
        // Detail windows (Physical Survey, Social Survey, Inhabited World)
        public bool DetailWindowsMdi { get; set; } = false;  // false = SDI (default)

        // Generation options
        public bool UseBenfordsLaw { get; set; } = false;    // Apply Benford's Law to population figures

        // ── Persistence ───────────────────────────────────────────────

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TravellerGenesis", "settings.json");

        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json, JsonOpts) ?? new AppSettings();
                }
            }
            catch { /* ignore and use defaults */ }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
                File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, JsonOpts));
            }
            catch { /* best-effort */ }
        }
    }
}
