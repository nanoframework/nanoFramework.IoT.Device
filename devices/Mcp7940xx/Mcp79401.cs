// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Common;

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Battery-Backed I2C Real-Time Clock/Calendar with Pre-Programmed EUI-48 MAC ID.
    /// </summary>
    public class Mcp79401 : Mcp7940n
    {
        /// <summary>
        /// Lower address of EEPROM memory region.
        /// </summary>
        private const byte LowerAddressBoundEEPROM = 0x0;

        /// <summary>
        /// Upper address of EEPROM memory region.
        /// </summary>
        private const byte UpperAddressBoundEEPROM = 0xEF;

        /// <summary>
        /// EEPROM Address of the EUI Node Address.
        /// </summary>
        protected const byte EuiAddress = 0xF0;

        /// <summary>
        /// Default I2C address for EEPROM.
        /// </summary>
        protected const byte EepromI2cAddress = 0b1010_0111;

        /// <summary>
        /// The underlying I2C device used for accessing the EEPROM address space.
        /// </summary>
        protected readonly I2cDevice _EepromI2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcp79401" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="clockSource">The clocks oscillator configuration.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public Mcp79401(I2cDevice i2cDevice, ClockSource clockSource)
            : base(i2cDevice, clockSource)
        {
            // Setup separate i2cDevice for connecting to devices eeprom address space.
            I2cConnectionSettings connectionSettings = new I2cConnectionSettings(_I2cDevice.ConnectionSettings.BusId, EepromI2cAddress, _I2cDevice.ConnectionSettings.BusSpeed);
            _EepromI2cDevice = new I2cDevice(connectionSettings);
        }

        /// <summary>
        /// Reads the EUI-48 MAC address.
        /// </summary>
        /// <returns>Returns the EUI-48 MAC address.</returns>
        public virtual byte[] ReadEUI()
        {
            byte[] writeBuffer = { EuiAddress };
            byte[] readBuffer = new byte[6];

            _EepromI2cDevice.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }

        #region EEPROM

        /// <summary>
        /// Sends unlock sequence to device to be able to write to the protected EEPROM block.
        /// </summary>
        private void UnlockEEPROM()
        {
            _I2cDevice.WriteByte(0x55);
            _I2cDevice.WriteByte(0xAA);
        }

        /// <summary>
        /// Verifies an address in within the EEPROM address space.
        /// </summary>
        /// <param name="address">The address to verify.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the addressable range for this device.</exception>
        internal void VerifyEEPROMAddress(byte address)
        {
            if (address < LowerAddressBoundEEPROM || address > UpperAddressBoundEEPROM)
            {
                throw new ArgumentOutOfRangeException(nameof(address));
            }
        }

        /// <summary>
        /// Reads a single byte from the devices EEPROM at the given address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>The byte read from the device.</returns>
        /// <remarks>
        /// Parameter <paramref name="address"/> must be in the range 0 to 63.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the addressable range for this device.</exception>
        public byte ReadByteFromEEPROM(byte address)
        {
            VerifyEEPROMAddress(address);

            return RegisterHelper.ReadRegister(_I2cDevice, address);
        }

        /// <summary>
        /// Writes a single byte to the devices EEPROM at the given address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The byte to be written into the device.</param>
        /// <remarks>
        /// Parameter <paramref name="address"/> must be in the range 0 to 63.
        /// </remarks>
        /// <returns>Returns the number of bytes written to the device.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the addressable range for this device.</exception>
        public uint WriteByteToEEPROM(byte address, byte value)
        {
            VerifyEEPROMAddress(address);

            UnlockEEPROM();
            return RegisterHelper.WriteRegister(_I2cDevice, address, value);
        }

        #endregion

        /// <inheritdoc/>
        public override void Dispose()
        {
            _EepromI2cDevice.Dispose();
        }
    }
}
