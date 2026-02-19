using System;
using System.Collections.Generic;

namespace TravellerSystemGenerator
{
    internal abstract class CelestialBody
    {
        public CelestialBodyType Type { get; set; }
        public string Designation { get; set; } = "";

        protected CelestialBody(CelestialBodyType type)
        {
            Type = type;
        }
    }

    internal class Moon
    {
        public string Size { get; set; } = "";      // Size code: R, S, 0-9, A-F, GS, GM
        public string Designation { get; set; } = ""; // a, b, c, etc.
        public int Diameter { get; set; } = 0;      // Diameter in km
        public string Composition { get; set; } = ""; // e.g., "Mostly Ice"
        public float Density { get; set; } = 0;     // Relative to Earth
        public float Gravity { get; set; } = 0;     // In Earth gravities
        public float Mass { get; set; } = 0;        // In Earth masses
        public float EscapeVelocity { get; set; } = 0; // In km/s
        public float Orbit { get; set; } = 0;           // In parent world diameters
        public float OrbitalPeriod { get; set; } = 0;   // In hours
        public float Eccentricity { get; set; } = 0;    // Orbital eccentricity (0-1)
        public bool IsRetrograde { get; set; } = false; // True if retrograde orbit
        public float OrbitDistanceKm { get; set; } = 0; // Orbital distance in km
        public string Atmosphere { get; set; } = "";    // Atmosphere code: 0-9, A-H
        public string AtmosphereComposition { get; set; } = ""; // Full atmosphere description with oxygen %
        public string WorldType { get; set; } = "";     // Frozen, Cold, Temperate, Hot, Boiling
        public float AtmosphericPressure { get; set; } = 0;  // In bars
        public int MeanTemperatureK { get; set; } = 0;       // In Kelvin
        public int MeanTemperatureC { get; set; } = 0;       // In Celsius
        public float HydrographicsCoverage { get; set; } = 0; // Percentage (0-100)
        public string HydrographicsCode { get; set; } = "";  // Code: 0-9, A
        public float BasicRotationRateHours { get; set; } = 0;  // Sidereal rotation period in hours (decimal)
        public float SolarDaysInLocalYear { get; set; } = 0;    // Number of solar days per orbital period
        public float SolarDayHours { get; set; } = 0;           // Length of a solar day in hours
        public float AxialTilt { get; set; } = 0;            // Axial tilt in degrees
        public string TidalLockStatus { get; set; } = "";    // Tidal lock status (e.g., "Locked to planet", "3:2 lock", etc.)
        public bool IsRetrogradeSpin { get; set; } = false;  // True if rotation is retrograde (from tidal lock)
        public float Albedo { get; set; } = 0;               // Reflectivity (0-1)
        public float Greenhouse { get; set; } = 0;           // Greenhouse effect factor
        public string SurfaceDistribution { get; set; } = "";  // Surface water distribution
        public float AxialTiltFactor { get; set; } = 0;      // sin(axial tilt)
        public float RotationFactor { get; set; } = 0;       // Based on solar day length
        public float GeographicFactor { get; set; } = 0;     // Based on hydrographics
        public float VarianceFactors { get; set; } = 0;      // Sum of tilt, rotation, geographic (0-1)
        public float AtmosphericFactor { get; set; } = 0;    // 1 + atmospheric pressure
        public float LuminosityModifier { get; set; } = 0;   // Variance / Atmospheric (0-1)
        public float HighLuminosity { get; set; } = 0;       // Luminosity * (1 + modifier)
        public float LowLuminosity { get; set; } = 0;        // Luminosity * (1 - modifier)
        public float NearAU { get; set; } = 0;               // Orbit * (1 - eccentricity)
        public float FarAU { get; set; } = 0;                // Orbit * (1 + eccentricity)
        public int HighTemperatureK { get; set; } = 0;       // High temperature in Kelvin
        public int LowTemperatureK { get; set; } = 0;        // Low temperature in Kelvin
        public int HighTemperatureC { get; set; } = 0;       // High temperature in Celsius
        public int LowTemperatureC { get; set; } = 0;        // Low temperature in Celsius

        // Tidal force tracking
        public List<TidalForceContribution> TidalForceContributions { get; set; } = new List<TidalForceContribution>();
        public float TotalTidalForce { get; set; } = 0;  // Total tidal force in meters

        // Seismology tracking
        public float ResidualSeismicStress { get; set; } = 0;      // Residual seismic stress
        public float TidalStressFactor { get; set; } = 0;          // Tidal stress factor (TotalTidalForce / 10)
        public float TidalHeatingEffects { get; set; } = 0;        // Tidal heating effects
        public float TotalSeismicStress { get; set; } = 0;         // Total seismic stress
        public int NumberOfMajorTectonicPlates { get; set; } = 0;  // Number of major tectonic plates

        // Native Lifeforms tracking
        public string AtmosphericTaint { get; set; } = "None";     // Atmospheric taint type
        public string AtmosphericIrritant { get; set; } = "None";  // Atmospheric irritant type
        public int BiomassRating { get; set; } = 0;                // Biomass rating (0+)
        public int BiocomplexityRating { get; set; } = 0;          // Biocomplexity rating (0-A+)
        public string BiocomplexityDescription { get; set; } = ""; // Description of biocomplexity level
        public string CurrentNativeSophont { get; set; } = "No";   // Yes/No for current native sophonts
        public bool ExtinctNativeSophont { get; set; } = false;    // True if evidence of extinct sophonts
        public int BiodiversityRating { get; set; } = 0;           // Biodiversity rating (1+)
        public int CompatibilityRating { get; set; } = 0;          // Compatibility rating (0+)
        public int ResourceRating { get; set; } = 0;               // Resource rating
        public int HabitabilityRating { get; set; } = 0;           // Habitability rating (0+)

        public Moon() { }
    }

    // Helper class to track individual tidal force contributions
    internal class TidalForceContribution
    {
        public string SourceName { get; set; } = "";      // Name of the body causing the tidal force
        public string SourceType { get; set; } = "";      // Type: "Star", "Gas Giant", "Planet", "Moon"
        public float TidalForce { get; set; } = 0;        // Tidal force contribution in meters
    }

    internal class GasGiant : CelestialBody
    {
        public float Mass { get; set; }           // Earth masses (stored in GasGiantMass)
        public float Radius { get; set; }         // Earth radii
        public float? Inclination { get; set; }   // Degrees (for Eccentric anomalous)
        public string? TrojanPosition { get; set; } // "L4" or "L5"
        public string Size { get; set; } = "";    // Size code: GS, GM, GL
        public int Diameter { get; set; }         // Diameter in Earth diameters (for ehex)
        public int GasGiantMass { get; set; }     // Mass in Earth masses
        public List<Moon> Moons { get; set; } = new List<Moon>();
        public int RingCount { get; set; } = 0;   // Number of planetary rings
        public float HillSphere { get; set; } = 0;          // In AU
        public float HillSpherePD { get; set; } = 0;        // In planetary diameters
        public float HillSphereMoonLimit { get; set; } = 0; // In planetary diameters
        public float RocheLimit { get; set; } = 0;          // In planetary diameters
        public float BasicRotationRateHours { get; set; } = 0;  // Sidereal rotation period in hours (decimal)
        public float SolarDaysInLocalYear { get; set; } = 0;    // Number of solar days per orbital period
        public float SolarDayHours { get; set; } = 0;           // Length of a solar day in hours
        public float Albedo { get; set; } = 0;               // Reflectivity (0-1)

        public GasGiant() : base(CelestialBodyType.GasGiant) { }
    }

    internal class TerrestrialPlanet : CelestialBody
    {
        public float Mass { get; set; }           // Solar masses (for orbital calculations)
        public float Radius { get; set; }         // Earth radii
        public float? Inclination { get; set; }   // Degrees (for Eccentric anomalous)
        public string? TrojanPosition { get; set; } // "L4" or "L5"
        public string Size { get; set; } = "";    // Size code: 0, S, 1-9, A-F
        public int Diameter { get; set; } = 0;    // Diameter in km
        public List<Moon> Moons { get; set; } = new List<Moon>();
        public int RingCount { get; set; } = 0;   // Number of planetary rings
        public string Composition { get; set; } = ""; // e.g., "Mostly Rock"
        public float Density { get; set; } = 0;   // Relative to Earth
        public float Gravity { get; set; } = 0;   // In Earth gravities
        public float WorldMass { get; set; } = 0; // In Earth masses
        public float EscapeVelocity { get; set; } = 0; // In km/s
        public float HillSphere { get; set; } = 0;          // In AU
        public float HillSpherePD { get; set; } = 0;        // In planetary diameters
        public float HillSphereMoonLimit { get; set; } = 0; // In planetary diameters
        public float RocheLimit { get; set; } = 0;          // In planetary diameters
        public string Atmosphere { get; set; } = "";        // Atmosphere code: 0-9, A-H
        public string AtmosphereComposition { get; set; } = ""; // Full atmosphere description with oxygen %
        public string WorldType { get; set; } = "";         // Frozen, Cold, Temperate, Hot, Boiling
        public float AtmosphericPressure { get; set; } = 0;     // In bars
        public int MeanTemperatureK { get; set; } = 0;          // In Kelvin
        public int MeanTemperatureC { get; set; } = 0;          // In Celsius
        public float HydrographicsCoverage { get; set; } = 0;   // Percentage (0-100)
        public string HydrographicsCode { get; set; } = "";     // Code: 0-9, A
        public float BasicRotationRateHours { get; set; } = 0;  // Sidereal rotation period in hours (decimal)
        public float SolarDaysInLocalYear { get; set; } = 0;    // Number of solar days per orbital period
        public float SolarDayHours { get; set; } = 0;           // Length of a solar day in hours
        public float AxialTilt { get; set; } = 0;               // Axial tilt in degrees
        public string TidalLockStatus { get; set; } = "";       // Tidal lock status (e.g., "Locked to star", "3:2 lock", etc.)
        public bool IsRetrogradeSpin { get; set; } = false;     // True if rotation is retrograde (from tidal lock)
        public float Albedo { get; set; } = 0;               // Reflectivity (0-1)
        public float Greenhouse { get; set; } = 0;           // Greenhouse effect factor
        public string SurfaceDistribution { get; set; } = "";  // Surface water distribution
        public float AxialTiltFactor { get; set; } = 0;      // sin(axial tilt)
        public float RotationFactor { get; set; } = 0;       // Based on solar day length
        public float GeographicFactor { get; set; } = 0;     // Based on hydrographics
        public float VarianceFactors { get; set; } = 0;      // Sum of tilt, rotation, geographic (0-1)
        public float AtmosphericFactor { get; set; } = 0;    // 1 + atmospheric pressure
        public float LuminosityModifier { get; set; } = 0;   // Variance / Atmospheric (0-1)
        public float HighLuminosity { get; set; } = 0;       // Luminosity * (1 + modifier)
        public float LowLuminosity { get; set; } = 0;        // Luminosity * (1 - modifier)
        public float NearAU { get; set; } = 0;               // Orbit * (1 - eccentricity)
        public float FarAU { get; set; } = 0;                // Orbit * (1 + eccentricity)
        public int HighTemperatureK { get; set; } = 0;       // High temperature in Kelvin
        public int LowTemperatureK { get; set; } = 0;        // Low temperature in Kelvin
        public int HighTemperatureC { get; set; } = 0;       // High temperature in Celsius
        public int LowTemperatureC { get; set; } = 0;        // Low temperature in Celsius

        // Tidal force tracking
        public List<TidalForceContribution> TidalForceContributions { get; set; } = new List<TidalForceContribution>();
        public float TotalTidalForce { get; set; } = 0;  // Total tidal force in meters

        // Seismology tracking
        public float ResidualSeismicStress { get; set; } = 0;      // Residual seismic stress
        public float TidalStressFactor { get; set; } = 0;          // Tidal stress factor (TotalTidalForce / 10)
        public float TidalHeatingEffects { get; set; } = 0;        // Tidal heating effects
        public float TotalSeismicStress { get; set; } = 0;         // Total seismic stress
        public int NumberOfMajorTectonicPlates { get; set; } = 0;  // Number of major tectonic plates

        // Native Lifeforms tracking
        public string AtmosphericTaint { get; set; } = "None";     // Atmospheric taint type
        public string AtmosphericIrritant { get; set; } = "None";  // Atmospheric irritant type
        public int BiomassRating { get; set; } = 0;                // Biomass rating (0+)
        public int BiocomplexityRating { get; set; } = 0;          // Biocomplexity rating (0-A+)
        public string BiocomplexityDescription { get; set; } = ""; // Description of biocomplexity level
        public string CurrentNativeSophont { get; set; } = "No";   // Yes/No for current native sophonts
        public bool ExtinctNativeSophont { get; set; } = false;    // True if evidence of extinct sophonts
        public int BiodiversityRating { get; set; } = 0;           // Biodiversity rating (1+)
        public int CompatibilityRating { get; set; } = 0;          // Compatibility rating (0+)
        public int ResourceRating { get; set; } = 0;               // Resource rating
        public int HabitabilityRating { get; set; } = 0;           // Habitability rating (0+)

        public TerrestrialPlanet() : base(CelestialBodyType.TerrestrialPlanet) { }
    }

    internal class PlanetoidBelt : CelestialBody
    {
        public float BeltSpan { get; set; } = 0;         // Orbital width in AU
        public int MType { get; set; } = 0;              // Metallic percentage (0-100)
        public int SType { get; set; } = 0;              // Silicate percentage (0-100)
        public int CType { get; set; } = 0;              // Carbonaceous percentage (0-100)
        public int Other { get; set; } = 0;              // Other composition percentage (0-100)
        public int Bulk { get; set; } = 0;               // Belt mass/density rating
        public int ResourceRating { get; set; } = 0;     // Mining value rating
        public int Size1Bodies { get; set; } = 0;        // Number of size 1 asteroids
        public int SizeSBodies { get; set; } = 0;        // Number of size S asteroids
        public string BeltProfile { get; set; } = "";    // Profile string for display
        public bool ContainsMainworld { get; set; } = false;  // True if this belt contains the mainworld
        public string? MainworldUWP { get; set; } = null;     // UWP of mainworld if contained in this belt

        public PlanetoidBelt() : base(CelestialBodyType.PlanetoidBelt) { }
    }

    internal class EmptyOrbit : CelestialBody
    {
        public EmptyOrbit() : base(CelestialBodyType.EmptyOrbit) { }
    }

    internal class Filled : CelestialBody
    {
        public Filled() : base(CelestialBodyType.Filled) { }
    }

    internal enum CelestialBodyType
    {
        Filled,            // Placeholder - will be assigned in future versions
        GasGiant,          // For future use
        PlanetoidBelt,     // For future use
        TerrestrialPlanet, // For future use
        EmptyOrbit,        // For future use
        Random,            // Anomalous orbit - random placement
        Eccentric,         // Anomalous orbit - high eccentricity
        Inclined,          // Anomalous orbit - inclined to ecliptic
        Retrograde,        // Anomalous orbit - retrograde motion
        Trojan             // Anomalous orbit - shares orbit with another body
    }
}
