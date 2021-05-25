// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vl53L0X
{
    /// <summary>
    /// Sensor have multiple modes, you can select one of the
    /// predefined mode using the SetPrecision method
    /// </summary>
    public enum Precision
    {
        /// <summary>Short range</summary>
        ShortRange = 0,

        /// <summary>Long range</summary>
        LongRange,

        /// <summary>High precision</summary>
        HighPrecision,
    }
}
