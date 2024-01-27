// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ds1621
{
    /// <summary>
    /// Ds1621 command set.
    /// </summary>
    internal enum Command : byte
    {
        /// <summary>
        /// Begins a temperature conversion.
        /// </summary>
        StartTemperatureConversion = 0xEE,

        /// <summary>
        /// Stops a temperature conversion.
        /// </summary>
        StopTemperatureConversion = 0x22
    }
}
