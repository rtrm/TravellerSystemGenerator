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
            // Initialize debug logging
            DebugLogger.Initialize();
            DebugLogger.Log("Starting Traveller System Generation");

            try
            {
                StarSystem starsystem = new StarSystem();
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

