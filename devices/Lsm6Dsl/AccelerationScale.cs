// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lsm6Dsl
{
    /// <summary>
    /// Accelerometer full-scale range.
    /// </summary>
    public enum AccelerationScale : byte
    {
        /// <summary>Plus or minus 2 g.</summary>
        Scale02G = 0b00,

        /// <summary>Plus or minus 16 g.</summary>
        Scale16G = 0b01,

        /// <summary>Plus or minus 4 g.</summary>
        Scale04G = 0b10,

        /// <summary>Plus or minus 8 g.</summary>
        Scale08G = 0b11,
    }
}