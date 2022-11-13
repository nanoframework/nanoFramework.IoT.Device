// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper
{
    /// <summary>
    /// Defines the display current power state.
    /// </summary>
    public enum PowerState
    {
        /// <summary>
        /// The display is in an unknown power state.
        /// </summary>
        Unknown,

        /// <summary>
        /// The display is powered on and ready to accept commands and data.
        /// </summary>
        PoweredOn,

        /// <summary>
        /// The display is powered off (sleeping) and depending on the sleep mode it may or may not allow some functions.
        /// </summary>
        PoweredOff,
    }

    /// <summary>
    /// Defines the supported color formats in the graphics library.
    /// </summary>
    public enum ColorFormat
    {
        /// <summary>
        /// 1-bit-per-pixel color format.
        /// </summary>
        Color1BitPerPixel,

        /// <summary>
        /// 2-bits-per-pixel. This is used by displays that support 3 colors by using 2 separate RAM.
        /// </summary>
        Color2BitPerPixel
    }

    /// <summary>
    /// Defines the supported rotations by the graphics APIs.
    /// </summary>
    public enum Rotation
    {
        /// <summary>
        /// No rotation. Uses the current display defaults.
        /// </summary>
        Default,

        /// <summary>
        /// The display is rotated 90 degrees clockwise.
        /// </summary>
        NinetyDegreesClockwise,

        /// <summary>
        /// The display is rotated 180 degrees clockwise.
        /// </summary>
        OneEightyDegreesClockwise,

        /// <summary>
        /// The display is rotated 270 degrees clockwise.
        /// </summary>
        TwoSeventyDegreesClockwise
    }
}
