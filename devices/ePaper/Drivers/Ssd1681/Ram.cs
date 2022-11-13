// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Drivers.Ssd1681
{
    /// <summary>
    /// Defines the SSD1681 RAMs. This is required to write image data to the correct RAM and control the color output.
    /// </summary>
    public enum Ram : byte
    {
        /// <summary>
        /// Specifies the black and white RAM area.
        /// </summary>
        BlackWhite = 0x00,

        /// <summary>
        /// Specifies the Colored RAM area (Red for SSD1681).
        /// </summary>
        Color = 0x01,
    }
}
