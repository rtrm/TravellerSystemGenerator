using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravellerSystemGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Check for help flags first
            if (args.Length > 0)
            {
                string firstArg = args[0].ToLower();
                if (firstArg == "-h" || firstArg == "-?" || firstArg == "/?" || firstArg == "/h" || firstArg == "--help")
                {
                    PrintUsage();
                    return;
                }
            }

            // Parse command line arguments
            int? seed = null;
            bool uniqueHtmlFilename = false;
            string? mainworldUWP = null;
            string? systemName = null;
            bool noMainworld = false;
            bool saveJson = true;
            string? loadFile = null;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg == "-u" || arg == "--unique")
                {
                    uniqueHtmlFilename = true;
                }
                else if (arg == "--no-json")
                {
                    saveJson = false;
                }
                else if (arg == "--load")
                {
                    if (i + 1 < args.Length)
                        loadFile = args[++i];
                    else
                    {
                        Console.WriteLine("Error: --load requires a file path.");
                        PrintUsage();
                        return;
                    }
                }
                else if (arg == "--no-mainworld")
                {
                    noMainworld = true;
                }
                else if (arg == "-m" || arg == "--mainworld")
                {
                    if (i + 1 < args.Length)
                    {
                        mainworldUWP = args[++i];

                        // Check if next arg is the optional world counts (3 digits like "222")
                        // If so, append it to the UWP with a space
                        if (i + 1 < args.Length &&
                            args[i + 1].Length == 3 &&
                            args[i + 1].All(char.IsDigit) &&
                            !args[i + 1].StartsWith("-"))
                        {
                            mainworldUWP += " " + args[++i];
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: --mainworld requires a UWP string.");
                        PrintUsage();
                        return;
                    }
                }
                else if (arg == "-n" || arg == "--name")
                {
                    if (i + 1 < args.Length)
                    {
                        systemName = args[++i];
                    }
                    else
                    {
                        Console.WriteLine("Error: --name requires a name string.");
                        PrintUsage();
                        return;
                    }
                }
                else if (int.TryParse(arg, out int parsedSeed))
                {
                    seed = parsedSeed;
                }
                else
                {
                    Console.WriteLine($"Error: Invalid argument '{arg}'.");
                    PrintUsage();
                    return;
                }
            }

            // Validate mainworld UWP if provided
            if (mainworldUWP != null)
            {
                string? validationError = ValidateMainworldUWP(mainworldUWP);
                if (validationError != null)
                {
                    Console.WriteLine($"Error: Invalid mainworld UWP - {validationError}");
                    return;
                }
            }

            // Initialize debug logging
            DebugLogger.Initialize();
            DebugLogger.Log($"Starting {Version.GetFullVersionString()}");
            DebugLogger.Log($"Version: {Version.VersionString}");
            if (seed.HasValue)
                DebugLogger.Log($"Command line seed: {seed.Value}");
            if (uniqueHtmlFilename)
                DebugLogger.Log($"Unique HTML filename enabled");

            try
            {
                if (loadFile != null)
                {
                    var snapshot = SystemSave.Load(loadFile);
                    _ = new StarSystem(snapshot, uniqueHtmlFilename);
                    DebugLogger.Log("System loaded from snapshot successfully");
                }
                else
                {
                    _ = new StarSystem(seed, uniqueHtmlFilename, mainworldUWP, systemName, noMainworld, saveJson);
                    DebugLogger.Log("System generation completed successfully");
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"ERROR: System generation failed - {ex.Message}");
                DebugLogger.Log($"Stack trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                // Close debug log
                DebugLogger.Close();
            }

            Console.WriteLine("\nDebug log written to: system_generation_debug.log");
        }

        static void PrintUsage()
        {
            Console.WriteLine($"{Version.GetFullVersionString()}");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  TravellerGenesisCLI [OPTIONS] [SEED]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -h, -?, /?, /h, --help");
            Console.WriteLine("                       Display this help message");
            Console.WriteLine();
            Console.WriteLine("  -u, --unique");
            Console.WriteLine("                       Generate unique HTML filename (system_[seed].html)");
            Console.WriteLine("                       Default: StarSystem.html");
            Console.WriteLine();
            Console.WriteLine("  -m, --mainworld UWP [COUNTS]");
            Console.WriteLine("                       Specify mainworld Universal World Profile");
            Console.WriteLine("                       Format: A123456-7 [890]");
            Console.WriteLine("                         A = Starport (A, B, C, D, E, X)");
            Console.WriteLine("                         1 = Size (0-F)");
            Console.WriteLine("                         2 = Atmosphere (0-H)");
            Console.WriteLine("                         3 = Hydrographics (0-A)");
            Console.WriteLine("                         4 = Population (0-C)");
            Console.WriteLine("                         5 = Government (0-F)");
            Console.WriteLine("                         6 = Law Level (0+)");
            Console.WriteLine("                         7 = Tech Level (0-G)");
            Console.WriteLine("                       Optional counts (3 digits):");
            Console.WriteLine("                         8 = Gas Giants (0-9)");
            Console.WriteLine("                         9 = Planetoid Belts (0-9)");
            Console.WriteLine("                         0 = Other Worlds/Terrestrials (0-9)");
            Console.WriteLine();
            Console.WriteLine("  -n, --name NAME");
            Console.WriteLine("                       Specify system name (use quotes for multiple words)");
            Console.WriteLine();
            Console.WriteLine("  --no-mainworld");
            Console.WriteLine("                       Disable automatic mainworld selection");
            Console.WriteLine("                       No mainworld will be selected or marked");
            Console.WriteLine();
            Console.WriteLine("  --no-json");
            Console.WriteLine("                       Skip saving the JSON snapshot file");
            Console.WriteLine("                       Default: saves to systems/StarSystem.json");
            Console.WriteLine();
            Console.WriteLine("  --load FILE");
            Console.WriteLine("                       Load a saved JSON snapshot and regenerate HTML");
            Console.WriteLine("                       FILE: path to a systems/*.json snapshot file");
            Console.WriteLine();
            Console.WriteLine("  SEED");
            Console.WriteLine("                       Optional integer seed for reproducible generation");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  TravellerGenesisCLI");
            Console.WriteLine("    Generate a random system");
            Console.WriteLine();
            Console.WriteLine("  TravellerGenesisCLI 12345");
            Console.WriteLine("    Generate system with seed 12345");
            Console.WriteLine();
            Console.WriteLine("  TravellerGenesisCLI -m B765432-9");
            Console.WriteLine("    Generate system with specified mainworld");
            Console.WriteLine();
            Console.WriteLine("  TravellerGenesisCLI -m B765432-9 223");
            Console.WriteLine("    Generate with mainworld: 2 gas giants, 2 belts, 3 terrestrials");
            Console.WriteLine();
            Console.WriteLine("  TravellerGenesisCLI -m D552325-3 222 -n Farhaven 54321");
            Console.WriteLine("    Generate \"Farhaven\" system with seed 54321 and specific mainworld");
            Console.WriteLine();
            Console.WriteLine("  TravellerGenesisCLI -u -n \"New Terra\"");
            Console.WriteLine("    Generate with unique HTML filename and multi-word name");
            Console.WriteLine();
            Console.WriteLine("  TravellerGenesisCLI --no-mainworld 12345");
            Console.WriteLine("    Generate system without automatic mainworld selection");
            Console.WriteLine();
            Console.WriteLine("Output:");
            Console.WriteLine("  - Console: System data in table format");
            Console.WriteLine("  - StarSystem.html: System overview (or system_[seed].html with -u)");
            Console.WriteLine("  - surveys/*.html: IISS Class IV Survey forms for worlds");
            Console.WriteLine("  - system_generation_debug.log: Debug information");
        }

        static string? ValidateMainworldUWP(string uwp)
        {
            // Remove spaces for validation
            string cleanUWP = uwp.Replace(" ", "");

            // Minimum length is 8 (A123456-7), maximum is 12 (A123456-7890 with dash) or 11 (A1234567890 without dash)
            if (cleanUWP.Length < 8)
                return "UWP too short. Minimum format is A123456-7";

            if (cleanUWP.Length > 12)
                return "UWP too long. Maximum format is A123456-7890";

            // Check starport (position 0)
            char starport = char.ToUpper(cleanUWP[0]);
            if (!"ABCDEX".Contains(starport))
                return $"Invalid starport code '{starport}'. Must be A, B, C, D, E, or X";

            // Check size (position 1) - 0-F
            if (!IsValidEhex(cleanUWP[1], 0, 15))
                return $"Invalid size code '{cleanUWP[1]}'. Must be 0-F";

            // Check atmosphere (position 2) - 0-H (0-17)
            if (!IsValidEhex(cleanUWP[2], 0, 17))
                return $"Invalid atmosphere code '{cleanUWP[2]}'. Must be 0-H";

            // Check hydrographics (position 3) - 0-A (0-10)
            if (!IsValidEhex(cleanUWP[3], 0, 10))
                return $"Invalid hydrographics code '{cleanUWP[3]}'. Must be 0-A";

            // Check population (position 4) - 0-C (0-12)
            if (!IsValidEhex(cleanUWP[4], 0, 12))
                return $"Invalid population code '{cleanUWP[4]}'. Must be 0-C";

            // Check government (position 5) - 0-F
            if (!IsValidEhex(cleanUWP[5], 0, 15))
                return $"Invalid government code '{cleanUWP[5]}'. Must be 0-F";

            // Check law level (position 6) - 0-9+ (ehex)
            if (!IsValidEhex(cleanUWP[6], 0, 35)) // Allow up to Z
                return $"Invalid law level code '{cleanUWP[6]}'. Must be valid ehex";

            // Position 7 should be '-' or a tech level digit
            if (cleanUWP.Length >= 8)
            {
                if (cleanUWP[7] == '-')
                {
                    // Format A123456-7, need tech level at position 8
                    if (cleanUWP.Length < 9)
                        return "Tech level missing after '-'";

                    if (!IsValidEhex(cleanUWP[8], 0, 16)) // 0-G
                        return $"Invalid tech level code '{cleanUWP[8]}'. Must be 0-G";

                    // Optional: gas giants, belts, other worlds at positions 9, 10, 11
                    if (cleanUWP.Length >= 10 && !char.IsDigit(cleanUWP[9]))
                        return $"Invalid gas giant count '{cleanUWP[9]}'. Must be a digit";

                    if (cleanUWP.Length >= 11 && !char.IsDigit(cleanUWP[10]))
                        return $"Invalid planetoid belt count '{cleanUWP[10]}'. Must be a digit";

                    if (cleanUWP.Length >= 12 && !char.IsDigit(cleanUWP[11]))
                        return $"Invalid other worlds count '{cleanUWP[11]}'. Must be a digit";
                }
                else
                {
                    // Format A1234567 or A1234567890 (no dash)
                    if (!IsValidEhex(cleanUWP[7], 0, 16)) // 0-G
                        return $"Invalid tech level code '{cleanUWP[7]}'. Must be 0-G";

                    // Optional: gas giants, belts, other worlds at positions 8, 9, 10
                    if (cleanUWP.Length >= 9 && !char.IsDigit(cleanUWP[8]))
                        return $"Invalid gas giant count '{cleanUWP[8]}'. Must be a digit";

                    if (cleanUWP.Length >= 10 && !char.IsDigit(cleanUWP[9]))
                        return $"Invalid planetoid belt count '{cleanUWP[9]}'. Must be a digit";

                    if (cleanUWP.Length >= 11 && !char.IsDigit(cleanUWP[10]))
                        return $"Invalid other worlds count '{cleanUWP[10]}'. Must be a digit";
                }
            }

            return null; // Valid
        }

        static bool IsValidEhex(char c, int min, int max)
        {
            c = char.ToUpper(c);
            int value;

            if (char.IsDigit(c))
            {
                value = c - '0';
            }
            else if (c >= 'A' && c <= 'Z')
            {
                value = c - 'A' + 10;
            }
            else
            {
                return false;
            }

            return value >= min && value <= max;
        }
    }
}

