// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.M5Pm1
{
    /// <summary>
    /// Output driver type of an M5PM1 GPIO pin.
    /// </summary>
    public enum GpioDrive
    {
        /// <summary>
        /// Push-pull output.
        /// </summary>
        PushPull = 0,

        /// <summary>
        /// Open-drain output.
        /// </summary>
        OpenDrain = 1,
    }
}
