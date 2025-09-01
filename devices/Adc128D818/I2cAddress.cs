// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc128D818
{
    /// <summary>
    /// ADC128D818 I2C address.
    /// </summary>
    /// <remarks>
    /// The I2C address is selected by the combination of voltage in both the A0 and A1 pins. Check the datasheet for more details.
    /// </remarks>
    public enum I2cAddress : byte
    {
        // Details in Table 2. Serial Bus Address Table

        /// <summary>
        /// A0 and A1 connect to GND.
        /// </summary>
        LowLow = 0x1D,

        /// <summary>
        /// A0 connects to GND and A1 connects to mid VDD.
        /// </summary>
        LowMid = 0x1E,

        /// <summary>
        /// A0 connects to GND and A1 connects to VDD.
        /// </summary>
        LowHigh = 0x1F,

        /// <summary>
        /// A0 connects to mid VDD and A1 connects to GND.
        /// </summary>
        MidLow = 0x2D,

        /// <summary>
        /// A0 and A1 connect to mid VDD.
        /// </summary>
        MidMid = 0x2E,

        /// <summary>
        /// A0 connects to mid VDD and A1 connects to VDD.
        /// </summary>
        MidHigh = 0x2F,

        /// <summary>
        /// A0 connects to VDD and A1 connects to GND.
        /// </summary>
        HighLow = 0x35,

        /// <summary>
        /// A0 connects to VDD and A1 connects to mid VDD.
        /// </summary>
        HighMid = 0x36,

        /// <summary>
        /// A0 and A1 connect to VDD.
        /// </summary>
        HighHigh = 0x37
    }
}
