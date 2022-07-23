// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// VHOLD voltage.
    /// </summary>
    public enum VholdVoltage
    {
        /// <summary>4.0 Volts.</summary>
        V4_0 = 0b0000_0000,

        /// <summary>4.1 Volts.</summary>
        V4_1 = 0b0000_1000,

        /// <summary>4.2 Volts.</summary>
        V4_2 = 0b0001_0000,

        /// <summary>4.3 Volts.</summary>
        V4_3 = 0b0001_1000,

        /// <summary>4.4 Volts.</summary>
        V4_4 = 0b0010_0000,

        /// <summary>4.5 Volts.</summary>
        V4_5 = 0b0010_1000,

        /// <summary>4.6 Volts.</summary>
        V4_6 = 0b0011_0000,

        /// <summary>4.7 Volts.</summary>
        V4_7 = 0b0011_1000,
    }
}
