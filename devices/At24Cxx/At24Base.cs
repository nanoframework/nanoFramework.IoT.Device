// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.At24Cxx
{
    /// <summary>
    /// Base class for common functionality of the At24C serices of devices.
    /// </summary>
    public abstract class At24Base
    {
        /// <summary>
        /// Default I2C address for At24Cx familly.
        /// </summary>
        public const byte DefaultI2cAddress = 0x50; // 01010000

        /// <summary>
        /// Underlying I2C device.
        /// </summary>
        private readonly I2cDevice _i2cDevice;

        /// <summary>
        /// Gets number of pages available on the device.
        /// </summary>
        public int PageCount { get; private set; }

        /// <summary>
        /// Gets device page size (in bytes).
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets available memory on the device (in bytes).
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Helper function to create a buffer with the first two bytes already populated by the device address.
        /// </summary>
        /// <param name="address">The device address.</param>
        /// <param name="length">The length of data the buffer is required to hold.</param>
        /// <returns>A buffer with the first two bytes populated with the device address.</returns>
        private static byte[] CreateWriteBuffer(int address, int length)
        {
            // Create buffer with an additional two bytes to hold the address.
            byte[] buffer = new byte[length + 2];

            // Store the address in the first two bytes.
            buffer[0] = (byte)((address >> 8) & 0xFF);
            buffer[1] = (byte)(address & 0xFF);

            return buffer;
        }

        /// <summary>
        /// Helper function to verify a memory address falls inside the accessible memory range for a At24C device.
        /// </summary>
        /// <param name="address">The memory address.</param>
        /// <param name="availableMemory">The amount of memory available on the device.</param>
        private static void VerifyMemoryAddress(int address, int availableMemory)
        {
            if (address < 0 || address > (availableMemory - 1))
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address must be within addressable memory range.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="At24Base" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="pageSize">The device page size in bytes.</param>
        /// <param name="pageCount">The number of pages on the device.</param>
        public At24Base(I2cDevice i2cDevice, ushort pageSize, ushort pageCount)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException(nameof(i2cDevice));
            }

            _i2cDevice = i2cDevice;
            PageSize = pageSize;
            PageCount = pageCount;
            Size = pageSize * pageCount;
        }

        /// <summary>
        /// Reads a single byte from the device at the address following the last byte read or written.
        /// </summary>
        /// <returns>The byte read from the device.</returns>
        public byte ReadByte()
        {
            return _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Reads a single byte from the device at the given address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>The byte read from the device.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the accessible range for this device.</exception>
        public byte ReadByte(int address)
        {
            VerifyMemoryAddress(address, Size);

            byte[] readBuffer = new byte[1];
            byte[] writeBuffer = CreateWriteBuffer(address, 0);

            _i2cDevice.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }

        /// <summary>
        /// Reads multiple bytes from the device starting from the given address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The bytes read from the device.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the accessible range for this device.</exception>
        public byte[] Read(int address, int length)
        {
            VerifyMemoryAddress(address, Size);

            byte[] readBuffer = new byte[length];
            byte[] writeBuffer = CreateWriteBuffer(address, 0);

            _i2cDevice.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }

        /// <summary>
        /// Writes a single byte to the device at the given address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The byte to be written into the device.</param>
        /// <returns>The number of bytes successfully written to the device.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the accessible range for this device.</exception>
        public uint WriteByte(int address, byte value)
        {
            VerifyMemoryAddress(address, Size);

            byte[] writeBuffer = CreateWriteBuffer(address, 1);
            writeBuffer[2] = value;

            I2cTransferResult result = _i2cDevice.Write(writeBuffer);

            // Account for memory address that was sent to device along with the data to be written
            return result.BytesTransferred - 2;
        }

        /// <summary>
        /// Writes multiple bytes to the device starting from the given address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The bytes to be written to the device.</param>
        /// <returns>The number of bytes successfully written to the device.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the accessible range for this device.</exception>
        public uint Write(int address, byte[] data)
        {
            VerifyMemoryAddress(address, Size);

            if (data.Length > PageSize)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Length of data to be written cannot exceed device page size.");
            }

            byte[] writeBuffer = CreateWriteBuffer(address, data.Length);
            data.CopyTo(writeBuffer, 2);

            I2cTransferResult result = _i2cDevice.Write(writeBuffer);

            // Account for memory address that was sent to device along with the data to be written
            return result.BytesTransferred - 2;
        }
    }
}
