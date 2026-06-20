// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;
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
        private const byte AuxBusyMask = 0x04;

        private const int AuxBusyPollDelayMs = 10;

        private const int AuxBusyPollRetries = 21;

        private const int AuxTransactionDelayMs = 1;

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
            // 1. Write data to AUX_WR_DATA (0x4F)
            // 2. Wait until AUX is not busy
            // 3. Write register address to AUX_WR_ADDR (0x4E) to trigger the aux write.
            SpanByte buff = new byte[2];

            buff[0] = (byte)Register.AuxWriteData;
            buff[1] = data;
            i2cDevice.Write(buff);

            WaitForAuxNotBusy(i2cDevice);

            buff[0] = (byte)Register.AuxWriteAddress;
            buff[1] = reg;
            i2cDevice.Write(buff);

            // Ensure the triggered AUX write transaction has completed.
            WaitForAuxNotBusy(i2cDevice);
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
            // In M5Unified/CoreS3 manual mode, AUX_IF_CONF is configured for single-byte access.
            // Read one byte per AUX transaction to avoid stale/invalid multi-byte AUX_DATA windows.
            for (int i = 0; i < readBytes.Length; i++)
            {
                byte currentReg = (byte)(reg + i);

                WaitForAuxNotBusy(i2cDevice);

                SpanByte buff = new byte[2];
                buff[0] = (byte)Register.AuxReadAddress;
                buff[1] = currentReg;
                i2cDevice.Write(buff);

                // Wait for AUX transaction completion after setting read address.
                WaitForAuxNotBusy(i2cDevice);

                // Bosch manual AUX read path waits briefly after setting AUX_RD_ADDR,
                // then reads AUX_DATA without requiring DRDY_AUX polling.
                Thread.Sleep(AuxTransactionDelayMs);

                i2cDevice.WriteByte((byte)Register.AuxData0);
                readBytes[i] = i2cDevice.ReadByte();
            }
        }

        private static void WaitForAuxNotBusy(I2cDevice i2cDevice)
        {
            for (int i = 0; i < AuxBusyPollRetries; i++)
            {
                i2cDevice.WriteByte((byte)Register.Status);
                byte status = i2cDevice.ReadByte();
                if ((status & AuxBusyMask) == 0)
                {
                    return;
                }

                Thread.Sleep(AuxBusyPollDelayMs);
            }

            throw new IOException("BMI270 AUX interface remained busy while accessing BMM150.");
        }
    }
}
