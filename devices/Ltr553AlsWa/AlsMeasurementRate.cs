// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// ALS measurement rate setting for the LTR-553ALS-WA.
    /// Must be equal to or larger than the integration time.
    /// </summary>
    public enum AlsMeasurementRate : byte
    {
        /// <summary>50 ms measurement rate.</summary>
        Rate50Ms = 0x00,

        /// <summary>100 ms measurement rate.</summary>
        Rate100Ms = 0x01,

        /// <summary>200 ms measurement rate.</summary>
        Rate200Ms = 0x02,

        /// <summary>500 ms measurement rate (default).</summary>
        Rate500Ms = 0x03,

        /// <summary>1000 ms measurement rate.</summary>
        Rate1000Ms = 0x04,

        /// <summary>2000 ms measurement rate.</summary>
        Rate2000Ms = 0x05,
    }
}
