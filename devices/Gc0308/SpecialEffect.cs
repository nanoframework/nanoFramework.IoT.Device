// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// Special image effect for the GC0308 camera sensor.
    /// Applied via the ISP (Image Signal Processor) pipeline.
    /// </summary>
    public enum SpecialEffect : byte
    {
        /// <summary>Normal mode (no effect applied).</summary>
        Normal = 0x00,

        /// <summary>Grayscale (black and white) output.</summary>
        Grayscale = 0x01,

        /// <summary>Sepia (antique brownish tone).</summary>
        Sepia = 0x02,

        /// <summary>Negative (color inversion).</summary>
        Negative = 0x03,

        /// <summary>Green tint effect.</summary>
        GreenTint = 0x04,

        /// <summary>Blue tint effect.</summary>
        BlueTint = 0x05,

        /// <summary>Red tint effect.</summary>
        RedTint = 0x06,
    }
}
