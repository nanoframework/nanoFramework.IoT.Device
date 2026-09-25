// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lis3Mdl
{
    /// <summary>
    /// Sensor performance mode.
    /// </summary>
    public enum PerformanceMode : byte
    {
        /// <summary>Low-power mode.</summary>
        LowPower = 0b00,

        /// <summary>Medium-performance mode.</summary>
        Medium = 0b01,

        /// <summary>High-performance mode.</summary>
        High = 0b10,

        /// <summary>Ultra-high-performance mode.</summary>
        UltraHigh = 0b11,
    }
}