# Release Notes - Traveller System Generator v2.0.0

## Major Release - Mainworld and System Name Parameters

This is a major release introducing command-line mainworld specification and system naming capabilities.

## New Features

### Command Line Mainworld Specification
- **Mainworld UWP Parameter** (`-m`, `--mainworld`)
  - Full UWP format support: `A123456-7`
  - Optional world counts: `A123456-7 890` (gas giants, belts, terrestrials)
  - Complete validation with detailed error messages
  - Example: `TravellerSystemsGenerator -m B765432-9 223`

### System Naming
- **Name Parameter** (`-n`, `--name`)
  - Support for single and multi-word system names
  - Displays in console and HTML output
  - Example: `TravellerSystemsGenerator -n "New Terra"`

### Intelligent Mainworld Placement
- **Size 0 Mainworlds**: Automatically placed in planetoid belts when available
- **Atmosphere 4-9 Mainworlds**: Must be placed in habitable zone
  - Can be standalone planets or gas giant moons in HZ
  - System regenerates if no HZ placement available
- **Flexible Placement**: Mainworlds can be standalone or orbiting gas giants

### Comprehensive Help System
- Multiple help flags: `-h`, `-?`, `/?`, `/h`, `--help`
- Detailed parameter descriptions
- UWP format breakdown with field explanations
- Multiple usage examples
- Output file documentation

## Improvements

### Display Enhancements
- Mainworld UWP shown in SAH/UWP column for planetoid belts
- Planetoid belt stats remain in Notes column
- Mainworld moons appear after parent gas giant (not before)
- Correct terrestrial count in system summary
- Fixed broken HTML links for planetoid belts containing mainworlds

### Bug Fixes
- Fixed Hill Sphere calculation (diameter conversion from Earth diameters to km)
- Fixed UWP parsing for space-separated optional counts
- Improved HZ availability checking for mainworld placement
- Enhanced UWP validation with maximum length of 12 characters

## Technical Details

- **Build**: Self-contained single-file executable (includes .NET runtime)
- **Size**: ~31 MB (compressed), ~71 MB (extracted)
- **Platform**: Windows 10/11 (64-bit)
- **Requirements**: No .NET installation required

## Installation

1. Download `TravellerSystemGenerator-v2.0.0-win-x64.zip`
2. Extract to a folder of your choice
3. Run `TravellerSystemsGenerator.exe` from command line

## Usage Examples

Generate a random system:
```
TravellerSystemsGenerator.exe
```

Generate with specific mainworld:
```
TravellerSystemsGenerator.exe -m B765432-9
```

Generate with mainworld and world counts:
```
TravellerSystemsGenerator.exe -m B765432-9 223
```
(2 gas giants, 2 planetoid belts, 3 terrestrials)

Generate named system with mainworld:
```
TravellerSystemsGenerator.exe -m D552325-3 222 -n Farhaven
```

Generate with reproducible seed:
```
TravellerSystemsGenerator.exe -m B765432-9 -n "Alpha Centauri" 12345
```

View all options:
```
TravellerSystemsGenerator.exe -h
```

## Output Files

- **Console**: System data in formatted tables
- **StarSystem.html**: Interactive system overview with clickable links
- **surveys/*.html**: IISS Class IV Survey forms for all terrestrial worlds and moons
- **system_generation_debug.log**: Detailed debug information

## Breaking Changes

None - this release is fully backward compatible with v1.x

## Known Issues

- One compiler warning about nullable reference in HandleTrojanOrbits (does not affect functionality)

## Upgrade Notes

This is a major version update (1.x → 2.0.0). The self-contained deployment means:
- No separate .NET installation required
- Larger file size (includes runtime)
- Completely portable - can run from any folder

## Contributors

- RTRM
- Claude Sonnet 4.5

## Links

- **Repository**: https://github.com/rtrm/TravellerSystemGenerator
- **Issues**: https://github.com/rtrm/TravellerSystemGenerator/issues
- **Previous Release**: v1.0.0

## Checksums

SHA256 checksums will be provided in the release assets.

---

*For detailed commit history, see the GitHub repository.*
