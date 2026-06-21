// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// ALS integration time setting for the LTR-553ALS-WA.
    /// Longer integration times give higher resolution but slower updates.
    /// </summary>
    public enum AlsIntegrationTime : byte
    {
        /// <summary>50 ms integration time.</summary>
        Integration50Ms = 0x01,

        /// <summary>100 ms integration time (default).</summary>
        Integration100Ms = 0x00,

        /// <summary>150 ms integration time.</summary>
        Integration150Ms = 0x04,

        /// <summary>200 ms integration time.</summary>
        Integration200Ms = 0x02,

        /// <summary>250 ms integration time.</summary>
        Integration250Ms = 0x05,

        /// <summary>300 ms integration time.</summary>
        Integration300Ms = 0x06,

        /// <summary>350 ms integration time.</summary>
        Integration350Ms = 0x07,

        /// <summary>400 ms integration time.</summary>
        Integration400Ms = 0x03,
    }
}
