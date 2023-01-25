// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.At24cxx
{
    /// <summary>
    /// Base class for common functionality of the At24c serices of devices.
    /// </summary>
    public abstract class At24Base : IDisposable
    {
        /// <summary>
        /// Default I2C address for At24cxx familly.
        /// </summary>
        public const byte DefaultI2cAddress = 0x50;

        /// <summary>
        /// The underlying I2C device used for communication.
        /// </summary>
        private readonly I2cDevice _i2cDevice;

        /// <summary>
        /// Gets the number of pages available on the device.
        /// </summary>
        public int PageCount { get; private set; }

        /// <summary>
        /// Gets the device page size (in bytes).
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Gets the available memory on the device (in bytes).
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="At24Base" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="pageSize">The device page size in bytes.</param>
        /// <param name="pageCount">The number of pages on the device.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="i2cDevice"/> is null.
        /// </exception>
        internal At24Base(I2cDevice i2cDevice, ushort pageSize, ushort pageCount)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException();
            }

            _i2cDevice = i2cDevice;
            PageSize = pageSize;
            PageCount = pageCount;
            Size = pageSize * pageCount;
        }

        /// <summary>
        /// Helper function to create a buffer pre-populated with the device address.
        /// </summary>
        /// <param name="address">The device address.</param>
        /// <param name="dataLength">The length of data the buffer is required to hold.</param>
        /// <returns>
        /// A buffer with the device address occupying the first two bytes.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="address"/> falls outside of the addressable range for this device.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="dataLength"/> exceeds the page size for this device.
        /// </exception>
        protected byte[] CreateWriteBuffer(int address, int dataLength)
        {
            if (address < 0 || address > (Size - 1))
            {
                throw new ArgumentOutOfRangeException();
            }

            if (dataLength > PageSize)
            {
                throw new ArgumentOutOfRangeException();
            }

            // Create buffer with an additional two bytes to hold the address.
            byte[] buffer = new byte[dataLength + 2];

            // At24cxx requires the memory address to be in the first two bytes preceding data.
            buffer[0] = (byte)((address >> 8) & 0xFF);
            buffer[1] = (byte)(address & 0xFF);

            return buffer;
        }

        /// <summary>
        /// Reads a single byte from the device at the address following the last byte read or written.
        /// </summary>
        /// <returns>
        /// The byte read from the device.
        /// </returns>
        public byte ReadByte()
        {
            return _i2cDevice.ReadByte();
        }

        /// <summary>
        /// Reads a single byte from the device at the given address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>
        /// The byte read from the device.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="address"/> falls outside of the addressable range for this device.
        /// </exception>
        public byte ReadByte(int address)
        {
            byte[] writeBuffer = CreateWriteBuffer(address, 0);
            byte[] readBuffer = new byte[1];

            _i2cDevice.WriteRead(writeBuffer, readBuffer);

            return readBuffer[0];
        }

        /// <summary>
        /// Reads multiple bytes from the device starting from the given address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>
        /// The bytes read from the device.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="address"/> falls outside of the addressable range for this device.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the length of data to be read is zero or will cause the data read to fall outside of the addressable range for this device.
        /// </exception>
        public byte[] Read(int address, int length)
        {
            if (length <= 0 || (address + length) > (Size - 1))
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] writeBuffer = CreateWriteBuffer(address, 0);
            byte[] readBuffer = new byte[length];

            _i2cDevice.WriteRead(writeBuffer, readBuffer);

            return readBuffer;
        }

        /// <summary>
        /// Writes a single byte to the device at the given address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The byte to be written into the device.</param>
        /// <returns>
        /// The number of bytes successfully written to the device.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="address"/> falls outside of the addressable range for this device.
        /// </exception>
        public uint WriteByte(int address, byte value)
        {
            byte[] writeBuffer = CreateWriteBuffer(address, 1);
            writeBuffer[2] = value;

            I2cTransferResult result = _i2cDevice.Write(writeBuffer);

            // Account for memory address that was sent to the device along with the data to be written
            return result.BytesTransferred - 2;
        }

        /// <summary>
        /// Writes multiple bytes to the device starting from the given address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="data">The bytes to be written to the device.</param>
        /// <returns>The number of bytes successfully written to the device.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="address"/> falls outside of the addressable range for this device.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when length of data to be written exceeds the page size for this device.
        /// </exception>
        public uint Write(int address, byte[] data)
        {
            byte[] writeBuffer = CreateWriteBuffer(address, data.Length);
            data.CopyTo(writeBuffer, 2);

            I2cTransferResult result = _i2cDevice.Write(writeBuffer);

            // Account for memory address that was sent to the device along with the data to be written
            return result.BytesTransferred - 2;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
