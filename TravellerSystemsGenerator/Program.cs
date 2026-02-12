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
            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out int parsedSeed))
                {
                    seed = parsedSeed;
                }
                else
                {
                    Console.WriteLine($"Error: Invalid seed value '{args[0]}'. Seed must be an integer.");
                    Console.WriteLine("Usage: TravellerSystemsGenerator [seed]");
                    return;
                }
            }

            // Initialize debug logging
            DebugLogger.Initialize();
            DebugLogger.Log($"Starting {Version.GetFullVersionString()}");
            DebugLogger.Log($"Version: {Version.VersionString}");
            if (seed.HasValue)
                DebugLogger.Log($"Command line seed: {seed.Value}");

            try
            {
                StarSystem starsystem = new StarSystem(seed);
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

