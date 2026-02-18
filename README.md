Version 2.0.0 Latest
This Traveller Star System Generator creates star systems based on the generation system published in the Mongoose Traveller World Builder's Handbook, written by Geir Lanesskog.. Traveller and the World Builder's Handbook are copyright by Mongoose Publishing

The program is a command line tool, see the usage information below. It outputs to the command line, as well as creating HTML files containing information about the generated system, these are located in the folder the application runs from.

This release includes the following features:

Star generation, including companions, class, mass, diameter, luminosity and orbital eccentricity
Generation of orbits and placement of planets, gas giants and Planetoid Belts
Generation of the habitable zone
Anomalous planets (eccentric, inclined, retrograde and trojan orbits)
Orbital periods
Significant moons
World gas giant and moon sizing
World and moon gravity, mass, orbital periods and day length
Planetoid Belt characteristics
World and moon atmospheres, including atmospheric pressure, Albedo and Hydrographics
Tidal locks for planets and moons
Mean, max and low temperature for Worlds and moons
Generation of systems for existing systems
HTML and console output
Command line help
I have not included

some of the details of atmospheres, particularly taint details, exotic details and atmospheric chemical composition
the more detailed aspects of temperature calculation
Seismology
Native Lifeforms (this is next on the list)
World Social Characteristics (this is also a priority)
Usage:
TravellerSystemsGenerator [OPTIONS] [SEED]

Options:
-h, -?, /?, /h, --help
Display this help message

-u, --unique
Generate unique HTML filename (system_[seed].html)
Default: StarSystem.html

-m, --mainworld UWP [COUNTS]
Specify mainworld Universal World Profile
Format: A123456-7 [890]
A = Starport (A, B, C, D, E, X)
1 = Size (0-F)
2 = Atmosphere (0-H)
3 = Hydrographics (0-A)
4 = Population (0-C)
5 = Government (0-F)
6 = Law Level (0+)
7 = Tech Level (0-G)
Optional counts (3 digits):
8 = Gas Giants (0-9)
9 = Planetoid Belts (0-9)
0 = Other Worlds/Terrestrials (0-9)

-n, --name NAME
Specify system name (use quotes for multiple words)

SEED
Optional integer seed for reproducible generation

Examples:
TravellerSystemsGenerator
Generate a random system

TravellerSystemsGenerator 12345
Generate system with seed 12345

TravellerSystemsGenerator -m B765432-9
Generate system with specified mainworld

TravellerSystemsGenerator -m B765432-9 223
Generate with mainworld: 2 gas giants, 2 belts, 3 terrestrials

TravellerSystemsGenerator -m D552325-3 222 -n Farhaven 54321
Generate "Farhaven" system with seed 54321 and specific mainworld

TravellerSystemsGenerator -u -n "New Terra"
Generate with unique HTML filename and multi-word name

Output:

Console: System data in table format
StarSystem.html: System overview (or system_[seed].html with -u)
surveys/*.html: IISS Class IV Survey forms for worlds
system_generation_debug.log: Debug information