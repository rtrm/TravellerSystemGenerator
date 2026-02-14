# Traveller System Generator v1.0.0

## First Major Release! 🚀

This is the first official release of the Traveller System Generator, a C# console application that generates star systems based on the Traveller World Builder's Handbook by Mongoose Publishing.

## Features

### Star Generation
- Primary stars with full spectral classification
- Companion star systems (binary, trinary, hierarchical)
- Star designations (A, B, C, Aa, Ab, etc.)
- Orbital mechanics and period calculations
- Minimum allowable orbits (MAO)
- Habitable zone center orbits (HZCO)

### World Generation
- **Gas Giants** - Size determination (GS, GM, GL) with mass and diameter
- **Planetoid Belts** - Asteroid belt placement
- **Terrestrial Planets** - Size codes (0, S, 1-9, A-F)
- Anomalous orbits (Eccentric, Inclined, Retrograde, Trojan)
- Orbital placement with eccentricity calculations

### Moons
- Significant moon generation based on world size
- Moon sizing (R, S, 0-9, A-F, GS, GM)
- Ring systems (R-sized moons)
- Moon count modifiers based on orbit and stellar environment

### Output Formats

#### Console Output
Table-based format with three sections:
- **STELLAR** - Summary counts and system stats
- **STARS** - Detailed star properties and orbital data
- **OBJECTS** - World designations, orbits, and notes

#### HTML Output
- Professional table formatting
- Automatic generation to system.html (default)
- Optional unique filenames with -u or --unique switch
- Earth symbol (⊕) for terrestrial planet masses
- Mass in Earth masses (ME) for gas giants

### Reproducibility
- Command line seed parameter for reproducible generation
- Same seed generates identical systems
- Debug logging for detailed generation tracking

## Usage

```bash
# Generate random system
TravellerSystemsGenerator.exe

# Generate with specific seed
TravellerSystemsGenerator.exe 12345

# Generate with unique HTML filename
TravellerSystemsGenerator.exe 12345 -u
# Creates: system_12345.html

# Without -u flag, overwrites system.html
TravellerSystemsGenerator.exe 67890
# Creates: system.html (overwrites previous)
```

## Command Line Options

```
TravellerSystemsGenerator [seed] [-u|--unique]
  seed           Optional seed value for reproducible generation
  -u, --unique   Generate unique HTML filename (system_[seed].html)
```

## Output Files
- system.html (or system_[seed].html with -u) - HTML formatted output
- system_generation_debug.log - Detailed generation log

## Version History
- 0.1-0.25: Pre-release development versions
- 1.0.0: First major release with complete feature set

## Based On
Traveller World Builder's Handbook by Mongoose Publishing

---

**Note:** This is a feature-complete release. Future updates will add atmospheric composition, hydrographics, population, government, law level, and other UWP (Universal World Profile) components.
