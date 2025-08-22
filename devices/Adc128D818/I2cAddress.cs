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
        Low_Low = 0x1D,

        /// <summary>
        /// A0 connects to GND and A1 connects to mid VDD.
        /// </summary>
        Low_Mid = 0x1E,

        /// <summary>
        /// A0 connects to GND and A1 connects to VDD.
        /// </summary>
        Low_High = 0x1F,

        /// <summary>
        /// A0 connects to mid VDD and A1 connects to GND.
        /// </summary>
        Mid_Low = 0x2D,

        /// <summary>
        /// A0 and A1 connect to mid VDD.
        /// </summary>
        Mid_Mid = 0x2E,

        /// <summary>
        /// A0 connects to mid VDD and A1 connects to VDD.
        /// </summary>
        Mid_High = 0x2F,

        /// <summary>
        /// A0 connects to VDD and A1 connects to GND.
        /// </summary>
        High_Low = 0x35,

        /// <summary>
        /// A0 connects to VDD and A1 connects to mid VDD.
        /// </summary>
        High_Mid = 0x36,

        /// <summary>
        /// A0 and A1 connect to VDD.
        /// </summary>
        High_High = 0x37
    }
}
