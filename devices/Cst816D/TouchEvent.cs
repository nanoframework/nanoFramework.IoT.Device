// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Cst816D
{
    /// <summary>
    /// Touch events encoded by the CST816D.
    /// </summary>
    public enum TouchEvent
    {
        /// <summary>
        /// A finger has touched the panel.
        /// </summary>
        PressDown = 0,

        /// <summary>
        /// A finger has left the panel.
        /// </summary>
        LiftUp = 1,

        /// <summary>
        /// A finger remains in contact with the panel.
        /// </summary>
        Contact = 2,

        /// <summary>
        /// No event is reported.
        /// </summary>
        None = 3,
    }
}
