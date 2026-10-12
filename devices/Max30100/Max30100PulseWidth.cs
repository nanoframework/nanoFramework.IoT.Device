// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100
{
    /// <summary>The LED pulse width and associated ADC resolution.</summary>
    public enum Max30100PulseWidth : byte
    {
        /// <summary>200 microseconds with 13-bit resolution.</summary>
        Microseconds200 = 0,

        /// <summary>400 microseconds with 14-bit resolution.</summary>
        Microseconds400 = 1,

        /// <summary>800 microseconds with 15-bit resolution.</summary>
        Microseconds800 = 2,

        /// <summary>1600 microseconds with 16-bit resolution.</summary>
        Microseconds1600 = 3,
    }
}
