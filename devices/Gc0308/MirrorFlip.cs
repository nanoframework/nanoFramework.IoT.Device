// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// Mirror and flip orientation for the GC0308 camera sensor output.
    /// Controlled via the analog mode 1 register (0x14) bits [1:0].
    /// </summary>
    public enum MirrorFlip : byte
    {
        /// <summary>Normal orientation (no mirror, no flip).</summary>
        None = 0x00,

        /// <summary>Horizontal mirror (left-right swap).</summary>
        HorizontalMirror = 0x01,

        /// <summary>Vertical flip (top-bottom swap).</summary>
        VerticalFlip = 0x02,

        /// <summary>Both horizontal mirror and vertical flip.</summary>
        Both = 0x03,
    }
}
