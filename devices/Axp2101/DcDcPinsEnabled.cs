// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Axp2101
{
    /// <summary>
    /// DCDC channel enable flags from DC_ONOFF_DVM_CTRL register (0x80), bits [4:0].
    /// </summary>
    [Flags]
    public enum DcDcPinsEnabled
    {
        /// <summary>DCDC1 enabled.</summary>
        DcDc1 = 0b0000_0001,

        /// <summary>DCDC2 enabled.</summary>
        DcDc2 = 0b0000_0010,

        /// <summary>DCDC3 enabled.</summary>
        DcDc3 = 0b0000_0100,

        /// <summary>DCDC4 enabled.</summary>
        DcDc4 = 0b0000_1000,

        /// <summary>DCDC5 enabled.</summary>
        DcDc5 = 0b0001_0000,
    }
}
