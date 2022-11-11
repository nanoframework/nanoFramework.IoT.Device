// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.ePaperGraphics
{
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
