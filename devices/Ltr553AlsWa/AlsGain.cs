// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// ALS gain setting for the LTR-553ALS-WA.
    /// Controls the sensitivity of the ambient light sensor.
    /// </summary>
    public enum AlsGain : byte
    {
        /// <summary>1X gain (default).</summary>
        Gain1X = 0x00,

        /// <summary>2X gain.</summary>
        Gain2X = 0x01,

        /// <summary>4X gain.</summary>
        Gain4X = 0x02,

        /// <summary>8X gain.</summary>
        Gain8X = 0x03,

        /// <summary>48X gain.</summary>
        Gain48X = 0x06,

        /// <summary>96X gain.</summary>
        Gain96X = 0x07,
    }
}
