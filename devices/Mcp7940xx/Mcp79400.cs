// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Common;

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Battery-Backed I2C Real-Time Clock/Calendar with SRAM and EEPROM.
    /// </summary>
    public class Mcp79400 : Mcp7940n
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mcp79400" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="clockSource">The clocks oscillator configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public Mcp79400(I2cDevice i2cDevice, ClockSource clockSource)
            : base(i2cDevice, clockSource)
        {
        }

        /// <summary>
        /// Provides access to the protected EEPROM of the Mcp79400.
        /// </summary>
        public class Eeprom : IDisposable
        {
            /// <summary>
            /// Upper address of EEPROM memory region.
            /// </summary>
            private const byte UpperAddressBound = 0xEF;

            /// <summary>
            /// Default I2C address for Mcp79400x protected EEPROM.
            /// </summary>
            public const byte DefaultI2cAddress = 0b1010_0111;

            /// <summary>
            /// The underlying I2C device used for accessing the EEPROM address space.
            /// </summary>
            protected readonly I2cDevice _i2cDevice;

            /// <summary>
            /// Initializes a new instance of the <see cref="Eeprom" /> class.
            /// </summary>
            /// <param name="i2cDevice">The I2C device to use for communication.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
            public Eeprom(I2cDevice i2cDevice)
            {
                if (i2cDevice == null)
                {
                    throw new ArgumentNullException();
                }

                _i2cDevice = i2cDevice;
            }

            #region EEPROM

            /// <summary>
            /// Reads a single byte from the devices EEPROM at the given address.
            /// </summary>
            /// <param name="address">The address to read from.</param>
            /// <returns>The byte read from the device.</returns>
            /// <remarks>
            /// Parameter <paramref name="address"/> must be in the range 0 to 63.
            /// </remarks>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the addressable range for this device.</exception>
            public byte ReadByte(byte address)
            {
                if (address > UpperAddressBound)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return RegisterHelper.ReadRegister(_i2cDevice, address);
            }

            /// <summary>
            /// Writes a single byte to the devices EEPROM at the given address.
            /// </summary>
            /// <param name="address">The address to write to.</param>
            /// <param name="value">The byte to be written into the device.</param>
            /// <remarks>
            /// Parameter <paramref name="address"/> must be in the range 0 to 63.
            /// </remarks>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the addressable range for this device.</exception>
            public void WriteByte(byte address, byte value)
            {
                if (address > UpperAddressBound)
                {
                    throw new ArgumentOutOfRangeException();
                }

                // Send the unlock sequence to device to be able to write to the protected EEPROM block.
                _i2cDevice.WriteByte(0x55);
                _i2cDevice.WriteByte(0xAA);

                RegisterHelper.WriteRegister(_i2cDevice, address, value);
            }

            #endregion

            /// <inheritdoc/>
            public void Dispose()
            {
                _i2cDevice.Dispose();
            }
        }
    }
}
