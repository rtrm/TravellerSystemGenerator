namespace TravellerGenesis
{
    internal static class Version
    {
        public const int Major = 1;
        public const int Minor = 0;
        public const int Build = 0;

        public static string VersionString => $"{Major}.{Minor}.{Build}";

        public static string GetFullVersionString() =>
            $"Traveller Genesis v{VersionString}";

        // Version History:
        // 1.0.0 - Initial release: MDI session list, System Overview, Survey,
        //         Inhabited World, Populated World Details forms
    }
}
