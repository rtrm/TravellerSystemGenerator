using System;
using System.IO;

namespace TravellerSystemGenerator
{
    /// <summary>
    /// Provides debug logging functionality for system generation.
    /// The log file is overwritten each time the application runs.
    /// </summary>
    internal static class DebugLogger
    {
        private static readonly string logFilePath = "system_generation_debug.log";
        private static StreamWriter? logWriter;
        private static readonly object lockObject = new object();

        /// <summary>
        /// Initializes the debug logger, creating/overwriting the log file.
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Close existing writer if any
                Close();

                // Create new log file (overwrite if exists)
                logWriter = new StreamWriter(logFilePath, false);
                logWriter.AutoFlush = true;

                Log("=".PadRight(70, '='));
                Log("Traveller System Generator - Debug Log");
                Log($"Session started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Log("=".PadRight(70, '='));
                Log("");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing debug log: {ex.Message}");
            }
        }

        /// <summary>
        /// Writes a message to the debug log.
        /// </summary>
        public static void Log(string message)
        {
            lock (lockObject)
            {
                try
                {
                    if (logWriter != null)
                    {
                        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                        logWriter.WriteLine($"[{timestamp}] {message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to debug log: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Writes a formatted message to the debug log.
        /// </summary>
        public static void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        /// <summary>
        /// Writes a section header to the debug log.
        /// </summary>
        public static void LogSection(string sectionName)
        {
            Log("");
            Log($"--- {sectionName} ---");
        }

        /// <summary>
        /// Writes a dice roll result to the debug log.
        /// </summary>
        public static void LogDiceRoll(int numberOfDice, int result, string purpose = "")
        {
            string purposeText = string.IsNullOrEmpty(purpose) ? "" : $" ({purpose})";
            Log($"Dice Roll: {numberOfDice}d6 = {result}{purposeText}");
        }

        /// <summary>
        /// Closes the debug log file.
        /// </summary>
        public static void Close()
        {
            lock (lockObject)
            {
                if (logWriter != null)
                {
                    try
                    {
                        Log("");
                        Log("=".PadRight(70, '='));
                        Log($"Session ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        Log("=".PadRight(70, '='));
                        logWriter.Close();
                        logWriter.Dispose();
                    }
                    catch { }
                    finally
                    {
                        logWriter = null;
                    }
                }
            }
        }
    }
}
