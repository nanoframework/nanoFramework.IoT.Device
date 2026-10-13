// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Apds9960
{
    /// <summary>
    /// A gesture recognized by the APDS9960.
    /// </summary>
    public enum Gesture
    {
        /// <summary>No gesture was recognized.</summary>
        None,

        /// <summary>An upward gesture.</summary>
        Up,

        /// <summary>A downward gesture.</summary>
        Down,

        /// <summary>A leftward gesture.</summary>
        Left,

        /// <summary>A rightward gesture.</summary>
        Right,
    }
}
