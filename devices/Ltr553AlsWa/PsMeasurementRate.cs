// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ltr553AlsWa
{
    /// <summary>
    /// PS measurement rate setting for the LTR-553ALS-WA.
    /// </summary>
    public enum PsMeasurementRate : byte
    {
        /// <summary>10 ms measurement rate.</summary>
        Rate10Ms = 0x08,

        /// <summary>50 ms measurement rate.</summary>
        Rate50Ms = 0x00,

        /// <summary>70 ms measurement rate.</summary>
        Rate70Ms = 0x01,

        /// <summary>100 ms measurement rate (default).</summary>
        Rate100Ms = 0x02,

        /// <summary>200 ms measurement rate.</summary>
        Rate200Ms = 0x03,

        /// <summary>500 ms measurement rate.</summary>
        Rate500Ms = 0x04,

        /// <summary>1000 ms measurement rate.</summary>
        Rate1000Ms = 0x05,

        /// <summary>2000 ms measurement rate.</summary>
        Rate2000Ms = 0x06,
    }
}
