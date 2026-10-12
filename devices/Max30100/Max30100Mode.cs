// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100
{
    /// <summary>The MAX30100 measurement mode.</summary>
    public enum Max30100Mode : byte
    {
        /// <summary>Heart-rate mode using the infrared LED.</summary>
        HeartRate = 0x02,

        /// <summary>Pulse-oximetry mode using both LEDs.</summary>
        SpO2 = 0x03,
    }
}
