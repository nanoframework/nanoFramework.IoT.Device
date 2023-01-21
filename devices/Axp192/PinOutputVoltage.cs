// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Axp192
{
    /// <summary>
    /// Global pin output voltage.
    /// </summary>
    public enum PinOutputVoltage
    {
        /// <summary>1.8 Volts.</summary>
        V1_8 = 0b0000_0000,

        /// <summary>1.9 Volts.</summary>
        V1_9 = 0b0001_0000,

        /// <summary>2.0 Volts.</summary>
        V2_0 = 0b0010_0000,

        /// <summary>2.1 Volts.</summary>
        V2_1 = 0b0011_0000,

        /// <summary>2.2 Volts.</summary>
        V2_2 = 0b0100_0000,

        /// <summary>2.3 Volts.</summary>
        V2_3 = 0b0101_0000,

        /// <summary>2.4 Volts.</summary>
        V2_4 = 0b0110_0000,

        /// <summary>2.5 Volts.</summary>
        V2_5 = 0b0111_0000,

        /// <summary>2?6 Volts.</summary>
        V2_6 = 0b1000_0000,

        /// <summary>2.7 Volts.</summary>
        V2_7 = 0b1001_0000,

        /// <summary>2.8 Volts.</summary>
        V2_8 = 0b1010_0000,

        /// <summary>2.9 Volts.</summary>
        V2_9 = 0b1011_0000,

        /// <summary>3.0 Volts.</summary>
        V3_0 = 0b1100_0000,

        /// <summary>3.1 Volts.</summary>
        V3_1 = 0b1101_0000,

        /// <summary>3.2 Volts.</summary>
        V3_2 = 0b1110_0000,

        /// <summary>3.3 Volts.</summary>
        V3_3 = 0b1111_0000,
    }
}
