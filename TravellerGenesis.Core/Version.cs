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

        public const int Major = 4;
        public const int Minor = 0;
        public const int Build = 0;

        public static string VersionString => $"{Major}.{Minor}.{Build}";

        public static string GetFullVersionString()
        {
            return $"Traveller Genesis CLI v{VersionString}";
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
        // 2.8.x - Additional Inhabited Worlds (TL-based probability, RR/HR modifiers, population generation, SAH+pop display)
        // 2.9.x - Factions, government profiles (centralisation/authority/structure/G-CAS), faction relationships, Populated World Details update
        // 2.10.x - Secondary world governments (independence/authority rolls), classifications, trade codes (Cy/Fa/Fp/Mb/Mi/Pe/Rb), trade code tooltips
        // 2.11.x - Judicial systems for all inhabited worlds/nations (Judicial System, Law Uniformity, Presumption of Innocence, Death Penalty, Judicial Profile)
        // 2.12.x - Law Level sub-categories (Weapons, Economics, Criminal, Private, Personal Rights) and Law Level Profile (O-WECPR) for all inhabited worlds/nations
        // 2.13.0 - Nations display: bold IDs, readable format, nation-specific tooltips; Gov 0 faction tooltip; gov-tooltip font fix
        // 2.13.1 - Responsive HTML: wider max-width (1800px, 95% width), table overflow scroll, media queries for narrow viewports
        // 2.14.x - Secondary world law level adjustments: Gov 6 captive table, Gov 1-3 dependency roll, Freeport -1 DM; Pe recalculated after LL change
        // 2.15.0 - Tech Level subcategories: High/Low Common TL, 12 subcategories (Energy→Novelty), TL Profile (H-L-abcde-fghi-jk-l), nation TL profiles for Gov 7
        // 2.15.1 - Inhabited world forms: separate GOVERNMENT/TECH LEVEL/LAW LEVEL tables, Tech Level (UWP) row, consolidated NATIONS section for Gov 7
        // 2.15.2 - Secondary world TL authority adjustment (trade code based: Cy/Fa/Fp/Mb/Mi/Pe/Rb, MSTL floor); PCR uses adjusted TL
        // 2.16.x - Cultural attributes (Diversity, Xenophilia, Uniqueness, Symbology, Cohesion, Progressiveness, Expansionism, Militancy) and Cultural Profile (DXUS-CPEM)
        // 2.16.2 - Secondary world cultural authority adjustment: 0-2 attributes rerolled (1/3 chance each), non-rerolled attributes adjusted by DM delta (no cascading)
        // 2.17.x - Bases/XBoatWaystation flags; economic calculations (Importance, RF, LF, IF, EF, RU, GWP, WTN, Inequality, Development Score, Tariffs); ECONOMICS section on HTML forms
        // 2.17.1 - STARPORT/BASES/TRAVEL ZONE section added to INHABITED WORLD form; BASES shows actual base data on both forms
        // 2.17.2 - STARPORT section restructured: Class, Highport?, Expected Weekly Traffic, Berthing Fees, Capacity, Shipyard, Annual Output, Bases (Navy/Scout/Military/Other)
        // 2.17.3 - Mainworld bases generated: Highport, Naval, Scout, Military, Corsair (dice rolls per WHB); docking space, shipyard, berthing fees calculated
        // 2.17.4 - Starport capacity: traffic table (imp ± WTN), highport/downport docking, build capacity (shipyard), annual shipyard output
        // 2.18.0 - Spaceports for non-mainworld worlds: roll class (H/G/F/Y), equivalent starport class, bases, capacity; AIW UWP uses spaceport class; system overview SAH prefix for uninhabited worlds
        // 2.19.0 - Military branches: Enforcement, Militia, Army, Wet Navy, Air Force, System Defence, Navy, Marine; common modifiers (Militancy/Faction Relationship); faction relationships generated for AIWs
        // 2.19.1 - Basic Military Budget: 2% × (1+EF/10) × (1+roll/10); DMs for gov/law/bases/militancy/branches; subordinate AIWs inherit mainworld DM (+6 for mil base or penal colony)
        // 3.0.0  - Major release: spaceports, military branches, military budget
        // 3.0.1  - JSON save/load: systems/*.json snapshots, --no-json flag, --load <file> option
        // 4.0.0  - Rebranded to Traveller Genesis; Core library extracted; generateFiles flag; GUI support (TravellerGenesis.exe)
    }
}
