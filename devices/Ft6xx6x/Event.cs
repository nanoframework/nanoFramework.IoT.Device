// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ft6xx6x
{
    /// <summary>
    /// The touch event type.
    /// </summary>
    public enum Event
    {
        /// <summary>Press down</summary>
        PressDown = 0,

        /// <summary>Lift up</summary>
        LiftUp = 1,

        /// <summary>Contact </summary>
        Contact = 2,

        /// <summary>No event</summary>
        NoEvent = 3,
    }
}
