// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Ags01db
{
    /// <summary>
    /// MEMS VOC Gas Sensor ASG01DB.
    /// </summary>
    [Interface("MEMS VOC Gas Sensor ASG01DB")]
    public class Ags01db : IDisposable
    {
        /// <summary>
        /// ASG01DB Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x11;

        // CRC const
        private const byte CrcPolynomial = 0x31;
        private const byte CrcInit = 0xFF;
        private I2cDevice _i2cDevice;

        private int _lastMeasurement = 0;

        /// <summary>
        /// ASG01DB VOC (Volatile Organic Compounds) Gas Concentration (ppm).
        /// </summary>
        [Telemetry]
        public Ratio Concentration => Ratio.FromPartsPerMillion(GetConcentration());

        /// <summary>
        /// ASG01DB Version.
        /// </summary>
        [Property]
        public byte Version => GetVersion();

        /// <summary>
        /// Initializes a new instance of the <see cref="Ags01db" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Ags01db(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <summary>
        /// Get ASG01DB VOC Gas Concentration.
        /// </summary>
        /// <returns>Concentration (ppm).</returns>
        private double GetConcentration()
        {
            // The time of two measurements should be more than 2s.
            while (DateTime.UtcNow.Ticks - _lastMeasurement < 2000)
            {
                Thread.Sleep((int)DateTime.UtcNow.Ticks - _lastMeasurement);
            }

            // Details in the Datasheet P5
            // Write command MSB, LSB
            SpanByte writeBuff = new byte[2]
            {
                (byte)Register.ASG_DATA_MSB, (byte)Register.ASG_DATA_LSB
            };

            // Return data MSB, LSB, CRC checksum
            SpanByte readBuff = new byte[3];

            _i2cDevice.Write(writeBuff);
            _i2cDevice.Read(readBuff);

            _lastMeasurement = (int)DateTime.UtcNow.Ticks;

            // CRC check error
            if (!CheckCrc8(readBuff.Slice(0, 2), 2, readBuff[2]))
            {
                return -1;
            }

            ushort res = BinaryPrimitives.ReadUInt16BigEndian(readBuff.Slice(0, 2));

            return res / 10.0;
        }

        /// <summary>
        /// Get ASG01DB Version.
        /// </summary>
        /// <returns>ASG01DB Version.</returns>
        private byte GetVersion()
        {
            // Details in the Datasheet P5
            // Write command MSB, LSB
            SpanByte writeBuff = new byte[2]
            {
                (byte)Register.ASG_VERSION_MSB, (byte)Register.ASG_VERSION_LSB
            };

            // Return version, CRC checksum
            SpanByte readBuff = new byte[2];

            _i2cDevice.Write(writeBuff);
            _i2cDevice.Read(readBuff);

            // CRC check error
            if (!CheckCrc8(readBuff.Slice(0, 1), 1, readBuff[1]))
            {
                return unchecked((byte)-1);
            }

            return readBuff[0];
        }

        /// <summary>
        /// 8-bit CRC Checksum Calculation.
        /// </summary>
        /// <param name="data">Raw Data.</param>
        /// <param name="length">Data Length.</param>
        /// <param name="crc8">Raw CRC8.</param>
        /// <returns>Checksum is true or false.</returns>
        private bool CheckCrc8(SpanByte data, int length, byte crc8)
        {
            // Details in the Datasheet P6
            byte crc = CrcInit;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];

                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ CrcPolynomial);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }

            if (crc == crc8)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null;
            }
        }
    }
}
