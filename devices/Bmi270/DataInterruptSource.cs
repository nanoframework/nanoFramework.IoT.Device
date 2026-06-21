// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// Data interrupt sources that can be mapped to INT1 or INT2 pins.
    /// Corresponds to bits in INT_MAP_DATA (0x58).
    /// INT1 uses bits [3:0], INT2 uses bits [7:4].
    /// </summary>
    [Flags]
    public enum DataInterruptSource : byte
    {
        /// <summary>No data interrupts.</summary>
        None = 0x00,

        /// <summary>FIFO full interrupt.</summary>
        FifoFull = 0x01,

        /// <summary>FIFO watermark interrupt.</summary>
        FifoWatermark = 0x02,

        /// <summary>Data ready interrupt (accelerometer and/or gyroscope).</summary>
        DataReady = 0x04,

        /// <summary>Error interrupt.</summary>
        Error = 0x08,
    }
}
