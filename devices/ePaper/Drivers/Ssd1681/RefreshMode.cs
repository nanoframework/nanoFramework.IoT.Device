// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Drivers.Ssd1681
{
    /// <summary>
    /// SSD1861 Supported Refresh Modes.
    /// </summary>
    public enum RefreshMode : byte
    {
        /// <summary>
        /// Causes the display to perform a full refresh of the panel (Display Mode 1).
        /// </summary>
        FullRefresh = 0xf7,

        /// <summary>
        /// Causes the display to perform a partial refresh of the panel (Display Mode 2).
        /// </summary>
        PartialRefresh = 0xff,
    }
}
