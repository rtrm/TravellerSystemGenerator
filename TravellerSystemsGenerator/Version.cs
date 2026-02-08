using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    internal static class Version
    {
        // Version format: Major.Minor.Build
        // Major: Always 0 (pre-release)
        // Minor: Increment with each feature addition
        // Build: Increment with each build

        public const int Major = 0;
        public const int Minor = 9;
        public const int Build = 1;

        public static string VersionString => $"{Major}.{Minor}.{Build}";

        public static string GetFullVersionString()
        {
            return $"Traveller System Generator v{VersionString}";
        }

        // Version History:
        // 0.1.x - Debug logging system
        // 0.2.x - Companion star types (Other, Random, Lesser, Sibling, Twin)
        // 0.3.x - Twin companion separate mass/diameter randomization
        // 0.4.x - Console output formatting improvements
        // 0.5.x - D/BD star property fixes, version system, close orbit fix
        // 0.6.x - Hierarchical companion stars (companions can have Companion orbit companions)
        // 0.7.x - Orbital period calculations (years/days/hours based on distance)
        // 0.8.x - Non-stellar object counts (gas giants, planetoid belts, terrestrial planets)
        // 0.9.x - Minimum allowable orbit calculations for stars
    }
}
