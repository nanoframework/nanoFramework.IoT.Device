// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Iot.Device
{
    /// <summary>
    /// Implementations of some functions missing in older .NET versions.
    /// </summary>
    internal static class MathExtensions
    {
        /// <summary>
        /// Returns val, limited to the range min-max (inclusive).
        /// </summary>
        /// <param name="val">The value to restrict.</param>
        /// <param name="min">The min value to compare against.</param>
        /// <param name="max">The max value to compare against.</param>
        /// <returns>Value between min and max values.</returns>
        public static double Clamp(double val, double min, double max)
        {
#if !NET5_0_OR_GREATER
            if (val < min)
            {
                return min;
            }

            if (val > max)
            {
                return max;
            }

            return val;
#else
            return Math.Clamp(val, min, max);
#endif
        }

        /// <summary>
        /// Returns val, limited to the range min-max (inclusive).
        /// </summary>
        /// <param name="val">The value to restrict.</param>
        /// <param name="min">The min value to compare against.</param>
        /// <param name="max">The max value to compare against.</param>
        /// <returns>Value between min and max values.</returns>
        public static int Clamp(int val, int min, int max)
        {
#if !NET5_0_OR_GREATER
            if (val < min)
            {
                return min;
            }

            if (val > max)
            {
                return max;
            }

            return val;
#else
            return Math.Clamp(val, min, max);
#endif
        }

        /// <summary>
        /// Returns val, limited to the range min-max (inclusive).
        /// </summary>
        /// <param name="val">The value to restrict.</param>
        /// <param name="min">The min value to compare against.</param>
        /// <param name="max">The max value to compare against.</param>
        /// <returns>Value between min and max values.</returns>
        public static byte Clamp(byte val, byte min, byte max)
        {
#if !NET5_0_OR_GREATER
            if (val < min)
            {
                return min;
            }

            if (val > max)
            {
                return max;
            }

            return val;
#else
            return Math.Clamp(val, min, max);
#endif
        }

        /// <summary>
        /// Returns val, limited to the range min-max (inclusive).
        /// </summary>
        /// <param name="val">The value to restrict.</param>
        /// <param name="min">The min value to compare against.</param>
        /// <param name="max">The max value to compare against.</param>
        /// <returns>Value between min and max values.</returns>
        public static long Clamp(long val, long min, long max)
        {
#if !NET5_0_OR_GREATER
            if (val < min)
            {
                return min;
            }

            if (val > max)
            {
                return max;
            }

            return val;
#else
            return Math.Clamp(val, min, max);
#endif
        }

        /// <summary>
        /// Returns val, limited to the range min-max (inclusive).
        /// </summary>
        /// <param name="val">The value to restrict.</param>
        /// <param name="min">The min value to compare against.</param>
        /// <param name="max">The max value to compare against.</param>
        /// <returns>Value between min and max values.</returns>
        public static uint Clamp(uint val, uint min, uint max)
        {
#if !NET5_0_OR_GREATER
            if (val < min)
            {
                return min;
            }

            if (val > max)
            {
                return max;
            }

            return val;
#else
            return Math.Clamp(val, min, max);
#endif
        }
    }
}