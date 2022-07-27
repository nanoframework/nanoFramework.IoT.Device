// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.At24C128C
{
    /// <summary>
    /// At24C128C - I2C EEPROM read/write.
    /// </summary>
    public class At24C128C
    {
        private int _address;
        private I2cDevice _memoryController;

        /// <summary>
        /// Initializes a new instance of the <see cref="At24C128C" /> class.
        /// </summary>
        /// <param name="address">The I2C address of the device.</param>
        /// <param name="i2cBus">The I2C bus where the device is connected to.</param>
        public At24C128C(int address, int i2cBus)
        {
            // Store I2C address
            _address = address;

            var settings = new I2cConnectionSettings(i2cBus, address);

            // Instantiate I2C controller
            _memoryController = I2cDevice.Create(settings);
        }

        /// <summary>
        /// Write at a specific address.
        /// </summary>
        /// <param name="memoryAddress">The address to write.</param>
        /// <param name="messageToSent">The byte buffer to write.</param>
        public void Write(ushort memoryAddress, byte[] messageToSent)
        {
            byte[] txBuffer = new byte[2 + messageToSent.Length];
            txBuffer[0] = (byte)((memoryAddress >> 8) & 0xFF);
            txBuffer[1] = (byte)(memoryAddress & 0xFF);
            messageToSent.CopyTo(txBuffer, 2);
            _memoryController.Write(txBuffer);
        }

        /// <summary>
        /// Read a specific address.
        /// </summary>
        /// <param name="memoryAddress">The address to read.</param>
        /// <param name="numOfBytes">The number of bytes to read.</param>
        /// <returns>The read elements.</returns>
        public byte[] Read(ushort memoryAddress, int numOfBytes)
        {
            byte[] rxBuffer = new byte[numOfBytes];

            // Device address is followed by the memory address (two words)
            // and must be sent over the I2C bus before data reception
            byte[] txBuffer = new byte[2];
            txBuffer[0] = (byte)((memoryAddress >> 8) & 0xFF);
            txBuffer[1] = (byte)(memoryAddress & 0xFF);
            _memoryController.WriteRead(txBuffer, rxBuffer);

            return rxBuffer;
        }            
    }
}