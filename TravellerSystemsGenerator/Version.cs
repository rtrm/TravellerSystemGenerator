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
        // Major: Increment for major releases
        // Minor: Increment with each feature addition
        // Build: Increment with each build

        public const int Major = 2;
        public const int Minor = 7;
        public const int Build = 8;

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
        // 0.10.x - Orbital availability calculations (max orbits, unavailable ranges)
        // 0.11.x - Habitable zone center orbit calculations
        // 0.12.x - Total available orbits and world assignment calculations
        // 0.13.x - System Baseline Number calculation and world zone placement
        // 0.14.x - Baseline orbit calculations for primary star
        // 0.15.x - Orbit placement for celestial objects
        // 0.16.x - Anomalous orbits (Random, Eccentric, Inclined, Retrograde, Trojan)
        // 0.17.x - World placement (Gas Giants, Planetoid Belts, Terrestrial Planets, Empty Orbits)
        // 0.18.x - Star designations (A, B, C for primary/secondary/tertiary, a/b for companions)
        // 0.19.x - World designations (Roman numerals, planetoid belt P-prefix, star-based designation)
        // 0.20.x - Table-based console output format (STELLAR, STARS, OBJECTS)
        // 0.21.x - Terrestrial world size determination (0, S, 1-9, A-F)
        // 0.22.x - Gas giant size determination (GS, GM, GL with diameter and mass)
        // 0.23.x - UWP formatting (SAH/UWP column, ?? suffix, ME suffix, improved HZ calculation)
        // 0.24.x - Random seed support (reproducible system generation via command line parameter)
        // 0.25.x - Significant moons (moon count, sizing, Sub column, moon sizes in Notes)
        // 0.26.x - HTML output generation (table-based format, terrestrial planet mass with ⊕ symbol)
        // 1.0.0 - First major release
        // 1.1.x - World and moon diameter calculations (Size-based with random variation)
        // 1.2.x - IISS Class IV Survey forms (HTML forms for terrestrial worlds and moons, clickable links)
        // 1.3.x - Planetoid belt characteristics (composition, bulk, resource rating, profile)
        // 1.4.x - Moon orbital characteristics (Hill Sphere, moon removal logic, orbits, orbital periods)
        // 1.5.x - Non-HZ atmosphere generation (exotic atmospheres for worlds outside habitable zone)
        // 1.6.x - Atmospheric pressure, oxygen fraction, temperature, and hydrographics calculations
        // 1.7.x - Rotation and day length (sidereal period, solar days per year, solar day length)
        // 1.8.x - Axial tilt and tidal lock calculations (complex DM system, effect table, rotation modifications)
        // 1.9.x - Temperature calculations (albedo, greenhouse, surface distribution, high/low temperatures, variance factors)
        // 1.10.x - Mainworld and system name command line parameters (UWP specification, world counts, placement)
        // 2.0.0 - Major release with mainworld UWP and system name parameters
        // 2.1.x - Seismology calculations (tidal forces, seismic stress, tectonic plates), Type column in system overview
        // 2.2.x - Native Lifeforms (biomass, biocomplexity, biodiversity, compatibility, resource, habitability ratings)
        // 2.3.x - Atmospheric Taints (low oxygen, radioactivity, biologic, gas mix, particulates, sulphur compounds, high oxygen)
        // 2.4.x - Automatic mainworld selection (habitability, sophonts, resource rating criteria with weighted selection)
        // 2.5.x - Initial UWP generation (population, government, law level, starport, tech level with minimum requirements)
        // 2.6.x - Sophont world UWP generation (when --no-mainworld flag used, generate UWPs for worlds/moons with native sophonts)
        // 2.7.x - Population details (trade codes, PCR, urbanisation, major cities with distribution algorithm, Populated World Details form)
    }
}
