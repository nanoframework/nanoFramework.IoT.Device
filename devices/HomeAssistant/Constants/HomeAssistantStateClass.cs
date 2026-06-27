// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Home Assistant state class constants for sensors.
    /// </summary>
    public static class HomeAssistantStateClass
    {
        /// <summary>Instantaneous reading (e.g., temperature, humidity).</summary>
        public const string Measurement = "measurement";

        /// <summary>Monotonic increasing total (e.g., cumulative energy).</summary>
        public const string Total = "total";

        /// <summary>Monotonic increasing total that never resets.</summary>
        public const string TotalIncreasing = "total_increasing";
    }
}
