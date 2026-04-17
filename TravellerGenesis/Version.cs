namespace TravellerGenesis
{
    internal static class Version
    {
        public const int Major = 2;
        public const int Minor = 0;
        public const int Build = 8;

        public static string VersionString => $"{Major}.{Minor}.{Build}";

        public static string GetFullVersionString() =>
            $"Traveller Genesis v{VersionString}";

        // Version History:
        // 1.0.0 - Initial release: MDI session list, System Overview, Survey,
        //         Inhabited World, Populated World Details forms
        // 1.1.0 - SDI detail windows; renamed Survey → Physical Survey,
        //         Populated World Details → Social Survey; fixed populated-world routing
        // 1.2.0 - Options dialog with MDI/SDI toggle for detail windows
        // 1.3.0 - WorldPropertiesForm: tabbed Properties sheet replacing separate
        //         detail forms (Physical Survey + Social Survey tabs per available data)
        // 1.4.0 - App icon (multi-size ICO); icon on all windows; HTML favicon
        // 1.5.0 - Single right-pane layout: replaced MDI children with embedded
        //         SystemOverviewPanel; ListView selection switches active system
        // 2.0.0 - Travellermap.com sector/subsector import; Hex/Allegiance/TravelZone
        //         display in overview panel and Properties sheet
        // 2.0.1 - Fix imported UWP (all three data stores patched)
        // 2.0.2 - Mainworld Name pre-populated from system name on import;
        //         sector list prefetched in background on startup
        // 2.0.3 - Session list groups: imported systems grouped by sector/subsector source
        // 2.0.4 - Toolbar icons (Random/UWP/Open/Import/Save); left panel header shows import source
        // 2.0.5 - Moon rows added to worlds grid; double-click opens Physical Survey
        // 2.0.6 - Populated moons only get grid rows; click Notes column for moon context menu
        // 2.0.7 - Benford's Law option for population figures; settings persisted to AppData JSON
        // 2.0.8 - Hover tooltips on profile/code fields (Gov, Law, TL, Judicial, Culture, Trade Codes)
        //         and on worlds grid cells (Gas Giant type, Belt profile, SAH/UWP column)
    }
}
