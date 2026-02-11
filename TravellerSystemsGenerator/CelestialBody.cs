using System;

namespace TravellerSystemGenerator
{
    internal abstract class CelestialBody
    {
        public CelestialBodyType Type { get; set; }

        protected CelestialBody(CelestialBodyType type)
        {
            Type = type;
        }
    }

    internal class GasGiant : CelestialBody
    {
        public float Mass { get; set; }           // Solar masses
        public float Radius { get; set; }         // Earth radii
        public float? Inclination { get; set; }   // Degrees (for Eccentric anomalous)
        public string? TrojanPosition { get; set; } // "L4" or "L5"

        public GasGiant() : base(CelestialBodyType.GasGiant) { }
    }

    internal class TerrestrialPlanet : CelestialBody
    {
        public float Mass { get; set; }           // Solar masses
        public float Radius { get; set; }         // Earth radii
        public float? Inclination { get; set; }   // Degrees (for Eccentric anomalous)
        public string? TrojanPosition { get; set; } // "L4" or "L5"

        public TerrestrialPlanet() : base(CelestialBodyType.TerrestrialPlanet) { }
    }

    internal class PlanetoidBelt : CelestialBody
    {
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
