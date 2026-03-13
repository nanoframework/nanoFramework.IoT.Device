// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Magnetometer;

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// Provides I2C access to a BMM150 magnetometer connected through the BMI270's
    /// auxiliary (secondary) I2C master interface.
    /// </summary>
    /// <remarks>
    /// On the M5Stack CoreS3, the BMM150 (address 0x10) is physically wired to the
    /// BMI270's auxiliary I2C bus. This adapter routes Bmm150 register reads and writes
    /// through the BMI270's AUX registers so the magnetometer appears as a normal I2C device.
    /// </remarks>
    public class Bmm150I2cBmi270 : Bmm150I2cBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bmm150I2cBmi270"/> class.
        /// </summary>
        /// <param name="auxDeviceAddress">
        /// The 7-bit I2C address of the BMM150 on the auxiliary bus (typically 0x10).
        /// </param>
        public Bmm150I2cBmi270(byte auxDeviceAddress)
        {
            if (auxDeviceAddress != 0x10)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(auxDeviceAddress),
                    "BMM150 on the BMI270 auxiliary bus is expected to use address 0x10.");
            }
        }

        /// <inheritdoc/>
        public override void WriteRegister(I2cDevice i2cDevice, byte reg, byte data)
        {
            // BMI270 manual-access write sequence:
            // 1. Write the target register address to AUX_WR_ADDR (0x4F)
            // 2. Write the data byte to AUX_WR_DATA (0x4E)
            // The BMI270 executes the write on the auxiliary bus automatically.
            SpanByte buff = new byte[2];

            buff[0] = (byte)Register.AuxWriteAddress;
            buff[1] = reg;
            i2cDevice.Write(buff);

            buff[0] = (byte)Register.AuxWriteData;
            buff[1] = data;
            i2cDevice.Write(buff);

            // Allow the aux transaction to complete
            Thread.Sleep(2);
        }

        /// <inheritdoc/>
        public override byte ReadByte(I2cDevice i2cDevice, byte reg)
        {
            SpanByte result = new byte[1];
            ReadBytes(i2cDevice, reg, result);
            return result[0];
        }

        /// <inheritdoc/>
        public override void ReadBytes(I2cDevice i2cDevice, byte reg, SpanByte readBytes)
        {
            // BMI270 manual-access read sequence:
            // For each chunk:
            // 1. Set AUX_RD_ADDR (0x4D) to the target register on the BMM150.
            // 2. Wait for the BMI270 to perform the read on the aux bus.
            // 3. Read back from AUX_DATA registers (0x04-0x0B, up to 8 bytes per transaction).
            int remaining = readBytes.Length;
            int destOffset = 0;
            byte currentReg = reg;

            while (remaining > 0)
            {
                int chunkLength = remaining > 8 ? 8 : remaining;

                SpanByte buff = new byte[2];
                buff[0] = (byte)Register.AuxReadAddress;
                buff[1] = currentReg;
                i2cDevice.Write(buff);

                // Allow time for the auxiliary read to complete
                Thread.Sleep(2);

                SpanByte auxData = new byte[chunkLength];
                i2cDevice.WriteByte((byte)Register.AuxData0);
                i2cDevice.Read(auxData);

                for (int i = 0; i < chunkLength; i++)
                {
                    readBytes[destOffset + i] = auxData[i];
                }

                remaining -= chunkLength;
                destOffset += chunkLength;
                unchecked
                {
                    currentReg += (byte)chunkLength;
                }
            }
        }
    }
}
