// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// Output pixel format for the GC0308 camera sensor.
    /// Values correspond to bits[3:0] of the output format register (0x24), except for Grayscale.
    /// </summary>
    public enum OutputFormat : byte
    {
        /// <summary>YCbCr 4:2:2 output (default). Byte order: Y0 Cb Y1 Cr.</summary>
        YCbCr422 = 0x02,

        /// <summary>RGB565 output. Byte order: [R4:R0 G5:G3] [G2:G0 B4:B0].</summary>
        Rgb565 = 0x06,

        /// <summary>Grayscale output (Y channel only). Uses full register value 0xB1.</summary>
        Grayscale = 0xB1,
    }
}
