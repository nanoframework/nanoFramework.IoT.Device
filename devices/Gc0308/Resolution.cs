// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// Resolution presets for the GC0308 camera sensor.
    /// These control the output window size via sub-sampling and windowing.
    /// </summary>
    public enum Resolution : byte
    {
        /// <summary>VGA resolution: 640 x 480 pixels (full sensor output).</summary>
        Vga640x480 = 0x00,

        /// <summary>QVGA resolution: 320 x 240 pixels (2x sub-sampling).</summary>
        Qvga320x240 = 0x01,

        /// <summary>QQVGA resolution: 160 x 120 pixels (4x sub-sampling).</summary>
        Qqvga160x120 = 0x02,

        /// <summary>CIF resolution: 352 x 288 pixels (windowed).</summary>
        Cif352x288 = 0x03,
    }
}
