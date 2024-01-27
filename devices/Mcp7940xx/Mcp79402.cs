// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Common;

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Battery-Backed I2C Real-Time Clock/Calendar with SRAM, EEPROM and pre-programmed EUI-64 MAC ID.
    /// </summary>
    public class Mcp79402 : Mcp79401
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mcp79402" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="clockSource">The clocks oscillator configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public Mcp79402(I2cDevice i2cDevice, ClockSource clockSource)
            : base(i2cDevice, clockSource)
        {
        }

        /// <summary>
        /// Provides access to the protected EEPROM and pre-programmed EUI-64 MAC ID of the Mcp79402.
        /// </summary>
        public new class Eeprom : Mcp79401.Eeprom
        {
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
            /// Reads the EUI-64 MAC address.
            /// </summary>
            /// <returns>Returns the EUI-64 MAC address.</returns>
            public override byte[] ReadEui()
            {
                byte[] writeBuffer = { EuiAddress };
                byte[] readBuffer = new byte[8];

                _i2cDevice.WriteRead(writeBuffer, readBuffer);

                return readBuffer;
            }
        }
    }
}
