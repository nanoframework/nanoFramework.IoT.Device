// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// PS LED peak current setting for the LTR-553ALS-WA.
    /// </summary>
    public enum LedPeakCurrent : byte
    {
        /// <summary>5 mA peak current.</summary>
        Current5mA = 0x00,

        /// <summary>10 mA peak current.</summary>
        Current10mA = 0x01,

        /// <summary>20 mA peak current.</summary>
        Current20mA = 0x02,

        /// <summary>50 mA peak current.</summary>
        Current50mA = 0x03,

        /// <summary>100 mA peak current (default).</summary>
        Current100mA = 0x04,
    }
}
