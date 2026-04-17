using System;

namespace TravellerGenesis
{
    /// <summary>
    /// Applies Benford's Law to population figures using the exact log₁₀(1 + 1/d) formula.
    /// Only applied to values ≥ 1,000.
    /// </summary>
    internal static class BenfordsLaw
    {
        // Cumulative probability thresholds for digits 1–9
        private static readonly double[] Thresholds = BuildThresholds();

        private static double[] BuildThresholds()
        {
            var t = new double[9];
            double cumulative = 0;
            for (int d = 1; d <= 9; d++)
            {
                cumulative += Math.Log10(1.0 + 1.0 / d);
                t[d - 1] = cumulative;
            }
            return t;
        }

        /// <summary>Returns a leading digit 1–9 sampled from Benford's distribution.</summary>
        public static int LeadingDigit(Random rng)
        {
            double r = rng.NextDouble();
            for (int d = 1; d <= 9; d++)
                if (r < Thresholds[d - 1]) return d;
            return 9;
        }

        /// <summary>
        /// Replaces the leading digit of <paramref name="value"/> with one drawn from
        /// Benford's distribution, keeping the same order of magnitude and trailing digits.
        /// Returns <paramref name="value"/> unchanged if it is below 1,000.
        /// </summary>
        public static long Apply(long value, Random rng)
        {
            if (value < 1_000) return value;

            // Find the magnitude (10^(digits-1))
            long magnitude = 1;
            long temp = value;
            while (temp >= 10) { temp /= 10; magnitude *= 10; }

            long remainder  = value % magnitude;
            int  newLeading = LeadingDigit(rng);
            return newLeading * magnitude + remainder;
        }
    }
}
