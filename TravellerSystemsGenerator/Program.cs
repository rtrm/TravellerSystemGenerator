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

            foreach (string arg in args)
            {
                if (arg == "-u" || arg == "--unique")
                {
                    uniqueHtmlFilename = true;
                }
                else if (int.TryParse(arg, out int parsedSeed))
                {
                    seed = parsedSeed;
                }
                else
                {
                    Console.WriteLine($"Error: Invalid argument '{arg}'.");
                    Console.WriteLine("Usage: TravellerSystemsGenerator [seed] [-u|--unique]");
                    Console.WriteLine("  seed           Optional seed value for reproducible generation");
                    Console.WriteLine("  -u, --unique   Generate unique HTML filename (system_[seed].html)");
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
                StarSystem starsystem = new StarSystem(seed, uniqueHtmlFilename);
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


    }
}

