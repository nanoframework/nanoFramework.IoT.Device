// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Common;

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Battery-Backed I2C Real-Time Clock/Calendar with SRAM, EEPROM and pre-programmed EUI-48 MAC ID.
    /// </summary>
    public class Mcp79401 : Mcp79400
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mcp79401" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="clockSource">The clocks oscillator configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public Mcp79401(I2cDevice i2cDevice, ClockSource clockSource)
            : base(i2cDevice, clockSource)
        {
        }

        /// <summary>
        /// Provides access to the protected EEPROM and pre-programmed EUI-48 MAC ID of the Mcp79401.
        /// </summary>
        public new class Eeprom : Mcp79400.Eeprom
        {
            /// <summary>
            /// EEPROM Address of the EUI Node Address.
            /// </summary>
            protected const byte EuiAddress = 0xF0;

            /// <summary>
            /// Initializes a new instance of the <see cref="Eeprom" /> class.
            /// </summary>
            /// <param name="i2cDevice">The I2C device to use for communication.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
            public Eeprom(I2cDevice i2cDevice)
                : base(i2cDevice)
            {
            }

            /// <summary>
            /// Reads the EUI-48 MAC address.
            /// </summary>
            /// <returns>
            /// Returns the EUI-48 MAC address.
            /// </returns>
            public virtual byte[] ReadEui()
            {
                byte[] writeBuffer = { EuiAddress };
                byte[] readBuffer = new byte[6];

                _i2cDevice.WriteRead(writeBuffer, readBuffer);

                return readBuffer;
            }
        }
    }
}
