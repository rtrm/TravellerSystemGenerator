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
            // Parse command line arguments
            int? seed = null;
            bool uniqueHtmlFilename = false;
            string? mainworldUWP = null;
            string? systemName = null;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg == "-u" || arg == "--unique")
                {
                    uniqueHtmlFilename = true;
                }
                else if (arg == "-m" || arg == "--mainworld")
                {
                    if (i + 1 < args.Length)
                    {
                        mainworldUWP = args[++i];
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
                StarSystem starsystem = new StarSystem(seed, uniqueHtmlFilename, mainworldUWP, systemName);
                DebugLogger.Log("System generation completed successfully");
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
            Console.WriteLine("Usage: TravellerSystemsGenerator [seed] [-u|--unique] [-m|--mainworld UWP] [-n|--name NAME]");
            Console.WriteLine("  seed                 Optional seed value for reproducible generation");
            Console.WriteLine("  -u, --unique         Generate unique HTML filename (system_[seed].html)");
            Console.WriteLine("  -m, --mainworld UWP  Specify mainworld UWP (format: A123456-7 890)");
            Console.WriteLine("                       A=Starport, 1=Size, 2=Atmosphere, 3=Hydrographics,");
            Console.WriteLine("                       4=Population, 5=Government, 6=Law, 7=Tech Level,");
            Console.WriteLine("                       8=Gas Giants (opt), 9=Belts (opt), 0=Other Worlds (opt)");
            Console.WriteLine("  -n, --name NAME      Specify system name (use quotes if it contains spaces)");
        }

        static string? ValidateMainworldUWP(string uwp)
        {
            // Remove spaces for validation
            string cleanUWP = uwp.Replace(" ", "");

            // Minimum length is 8 (A123456-7), maximum is 11 (A1234567890)
            if (cleanUWP.Length < 8)
                return "UWP too short. Minimum format is A123456-7";

            if (cleanUWP.Length > 11)
                return "UWP too long. Maximum format is A1234567890";

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

