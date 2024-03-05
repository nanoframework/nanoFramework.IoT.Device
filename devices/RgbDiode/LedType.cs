// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.RgbDiode
{
    /// <summary>
    /// Enum for led types.
    /// </summary>
    public enum LedType : byte
    {
        /// <summary>
        /// Common cathode (-).
        /// </summary>
        CommonCathode = 0,

        /// <summary>
        /// Common anode (+).
        /// </summary>
        CommonAnode = 1
    }
}
