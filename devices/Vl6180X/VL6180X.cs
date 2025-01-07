// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Diagnostics;
using UnitsNet;

namespace Iot.Device.VL6180X
{
    /// <summary>
    /// Represents VL6180X.
    /// </summary>
    public sealed class VL6180X : IDisposable
    {
        /// <summary>
        /// The default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x29;

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="VL6180X" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C Device.</param>
        /// <param name="shouldDispose">True to dispose the I2C Device at dispose.</param>
        public VL6180X(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Initialization of the sensor, include a long sequence of writing
        /// which is coming from the offical API with no more information on the
        /// registers and their functions. Few can be reversed engineer based on
        /// other functions but not all.
        /// </summary>
        public void Init()
        {
            var alreadyInitialized = ReadRegister(RegisterAddresses.SYSTEM__FRESH_OUT_OF_RESET);
            if (alreadyInitialized != 1)
            {
                return;
            }

            WriteRegister(0x207, 0x01);
            WriteRegister(0x208, 0x01);
            WriteRegister(0x096, 0x00);
            WriteRegister(0x097, 0xFD); // RANGE_SCALER = 253
            WriteRegister(0x0E3, 0x01);
            WriteRegister(0x0E4, 0x03);
            WriteRegister(0x0E5, 0x02);
            WriteRegister(0x0E6, 0x01);
            WriteRegister(0x0E7, 0x03);
            WriteRegister(0x0F5, 0x02);
            WriteRegister(0x0D9, 0x05);
            WriteRegister(0x0DB, 0xCE);
            WriteRegister(0x0DC, 0x03);
            WriteRegister(0x0DD, 0xF8);
            WriteRegister(0x09F, 0x00);
            WriteRegister(0x0A3, 0x3C);
            WriteRegister(0x0B7, 0x00);
            WriteRegister(0x0BB, 0x3C);
            WriteRegister(0x0B2, 0x09);
            WriteRegister(0x0CA, 0x09);
            WriteRegister(0x198, 0x01);
            WriteRegister(0x1B0, 0x17);
            WriteRegister(0x1AD, 0x00);
            WriteRegister(0x0FF, 0x05);
            WriteRegister(0x100, 0x05);
            WriteRegister(0x199, 0x05);
            WriteRegister(0x1A6, 0x1B);
            WriteRegister(0x1AC, 0x3E);
            WriteRegister(0x1A7, 0x1F);
            WriteRegister(0x030, 0x00);
            WriteRegister(RegisterAddresses.SYSTEM__FRESH_OUT_OF_RESET, 0);
        }

        /// <summary>
        /// Reads the range measurement value from the sensor.
        /// </summary>
        /// <returns>The measured range value.</returns>
        public Length ReadRange()
        {
            WriteRegister(RegisterAddresses.SYSRANGE__START, 0x01);
            var range = ReadRegister(RegisterAddresses.RESULT__RANGE_VAL);
            WriteRegister(RegisterAddresses.SYSTEM__INTERRUPT_CLEAR, 0x01);
            return Length.FromMillimeters(range);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }

        private void WriteRegister(RegisterAddresses reg, byte param)
        {
            WriteRegister((ushort)reg, param);
        }

        private void WriteRegister(ushort reg, byte param)
        {
            var buffer = new byte[3];
            BinaryPrimitives.WriteUInt16BigEndian(buffer, reg);
            buffer[2] = param;
            Debug.WriteLine($"Writing to register 0x{reg:X4}: 0x{param:X2}");

            var result = _i2cDevice.Write(buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException("I2C write failed");
            }
        }

        private byte ReadRegister(RegisterAddresses reg)
        {
            var writeBuffer = new byte[2];
            var readBuffer = new byte[1];
            BinaryPrimitives.WriteUInt16BigEndian(writeBuffer, (ushort)reg);
            Debug.WriteLine($"Writing register address: 0x{writeBuffer[0]:X2} 0x{writeBuffer[1]:X2}");

            var result = _i2cDevice.WriteRead(writeBuffer, readBuffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException("I2C write failed while setting register address");
            }

            Debug.WriteLine($"Read data from register 0x{reg:X4}: 0x{readBuffer[0]:X2}");
            return readBuffer[0];
        }
    }
}