Version 3.0.0

This release includes the world social characteristics, completing the system generation system published in the World Builder's Handbook (WBH)

The WBH does assume that a referee is there to make some decisions during the system creation process, and to allow full automation, I have chosen a route to go or randomised the result.

You can use the program to either generate a system completely, or to take an existing UWP and have the program build a system around it, and flesh out some of the physical and social details.

It is a command line tool, written for Windows. When run it outputs the basic details of the system to the console, and also generates several html files. Open StarSystem.html in the directory the program is located in, and this has links to the more detailed screens.

Roy Martin
4/4/2026

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

--no-mainworld
Disable automatic mainworld selection
No mainworld will be selected or marked

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

TravellerSystemsGenerator --no-mainworld 12345
Generate system without automatic mainworld selection

Output:

Console: System data in table format
StarSystem.html: System overview (or system_[seed].html with -u)
surveys/*.html: IISS Class IV Survey forms for worlds
system_generation_debug.log: Debug information
Installation
Run TravellerSystemGenerator-3.0.0-Setup.exe — no .NET runtime required (self-contained).

Based On
Traveller World Builder's Handbook by Mongoose Publishing
