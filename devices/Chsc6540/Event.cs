// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Chsc6540
{
    /// <summary>
    /// The touch event type.
    /// </summary>
    public enum Event
    {
        /// <summary>No event.</summary>
        NoEvent = 0,

        /// <summary>Press down.</summary>
        PressDown,

        /// <summary>Lift up.</summary>
        LiftUp,
    }
}
