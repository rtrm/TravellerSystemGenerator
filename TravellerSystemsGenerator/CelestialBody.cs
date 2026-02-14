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

        public Moon() { }
    }

    internal class GasGiant : CelestialBody
    {
        public float Mass { get; set; }           // Solar masses
        public float Radius { get; set; }         // Earth radii
        public float? Inclination { get; set; }   // Degrees (for Eccentric anomalous)
        public string? TrojanPosition { get; set; } // "L4" or "L5"
        public string Size { get; set; } = "";    // Size code: GS, GM, GL
        public int Diameter { get; set; }         // Diameter in Earth diameters (for ehex)
        public int GasGiantMass { get; set; }     // Mass in Earth masses
        public List<Moon> Moons { get; set; } = new List<Moon>();
        public int RingCount { get; set; } = 0;   // Number of planetary rings

        public GasGiant() : base(CelestialBodyType.GasGiant) { }
    }

    internal class TerrestrialPlanet : CelestialBody
    {
        public float Mass { get; set; }           // Solar masses
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
