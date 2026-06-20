// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;

namespace Iot.Device.Paj7620
{
    /// <summary>
    /// Gestures recognized by the PAJ7620 sensor.
    /// </summary>
    [Flags]
    public enum Gesture
    {
        /// <summary>
        /// No gesture detected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Upward gesture.
        /// </summary>
        Up = 0x01,

        /// <summary>
        /// Downward gesture.
        /// </summary>
        Down = 0x02,

        /// <summary>
        /// Left gesture.
        /// </summary>
        Left = 0x04,

        /// <summary>
        /// Right gesture.
        /// </summary>
        Right = 0x08,

        /// <summary>
        /// Forward gesture.
        /// </summary>
        Forward = 0x10,

        /// <summary>
        /// Backward gesture.
        /// </summary>
        Backward = 0x20,

        /// <summary>
        /// Clockwise rotation gesture.
        /// </summary>
        Clockwise = 0x40,

        /// <summary>
        /// Counter-clockwise rotation gesture.
        /// </summary>
        CounterClockwise = 0x80,

        /// <summary>
        /// Wave gesture.
        /// </summary>
        Wave = 0x100,
    }
}
