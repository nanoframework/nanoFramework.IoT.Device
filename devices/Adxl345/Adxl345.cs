// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.Model;
using System.Device.Spi;
using System.Numerics;

namespace Iot.Device.Adxl345
{
    /// <summary>
    /// SPI Accelerometer ADX1345.
    /// </summary>
    [Interface("SPI Accelerometer ADX1345")]
    public class Adxl345 : IDisposable
    {
        private const int Resolution = 1024; // All g ranges resolution
        private readonly byte _gravityRangeByte;
        private readonly int _range;

        private SpiDevice _sensor;

        #region SpiSetting

        /// <summary>
        /// ADX1345 SPI Clock Frequency.
        /// </summary>
        public const int SpiClockFrequency = 5000000;

        /// <summary>
        /// ADX1345 SPI Mode.
        /// </summary>
        public const SpiMode SpiMode = System.Device.Spi.SpiMode.Mode3;

        #endregion

        /// <summary>
        /// Read Acceleration from ADXL345.
        /// </summary>
        [Telemetry]
        public Vector3 Acceleration => ReadAcceleration();

        /// <summary>
        /// Initializes a new instance of the <see cref="Adxl345" /> class. SPI Accelerometer ADX1345.
        /// </summary>
        /// <param name="sensor">The communications channel to a device on a SPI bus.</param>
        /// <param name="gravityRange">Gravity Measurement Range.</param>
        public Adxl345(SpiDevice sensor, GravityRange gravityRange)
        {
            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
            _range = ConvertGravityRangeToInt(gravityRange);
            _gravityRangeByte = (byte)gravityRange;
            Initialize();
        }

        private static int ConvertGravityRangeToInt(GravityRange gravityRange)
        {
            switch (gravityRange)
            {
                case GravityRange.Range02:
                    return 4;
                case GravityRange.Range04:
                    return 8;
                case GravityRange.Range08:
                    return 16;
                case GravityRange.Range16:
                    return 32;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Initialize ADXL345.
        /// </summary>
        private void Initialize()
        {
            Span<byte> dataFormat = new byte[]
            {
                (byte)Register.ADLX_DATA_FORMAT, _gravityRangeByte
            };
            Span<byte> powerControl = new byte[]
            {
                (byte)Register.ADLX_POWER_CTL, 0x08
            };

            _sensor.Write(dataFormat);
            _sensor.Write(powerControl);
        }

        /// <summary>
        /// Read data from ADXL345.
        /// </summary>
        /// <returns>Acceleration as Vector3.</returns>
        private Vector3 ReadAcceleration()
        {
            int units = Resolution / _range;

            Span<byte> readBuf = new byte[6 + 1];
            Span<byte> regAddrBuf = new byte[1 + 6];

            regAddrBuf[0] = (byte)(Register.ADLX_X0 | Register.ADLX_SPI_RW_BIT | Register.ADLX_SPI_MB_BIT);
            _sensor.TransferFullDuplex(regAddrBuf, readBuf);
            Span<byte> readData = readBuf.Slice(1);

            short accelerationX = BinaryPrimitives.ReadInt16LittleEndian(readData.Slice(0, 2));
            short accelerationY = BinaryPrimitives.ReadInt16LittleEndian(readData.Slice(2, 2));
            short accelerationZ = BinaryPrimitives.ReadInt16LittleEndian(readData.Slice(4, 2));

            Vector3 accel = new Vector3
            {
                X = (float)accelerationX / units, Y = (float)accelerationY / units, Z = (float)accelerationZ / units
            };

            return accel;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            if (_sensor != null && _sensor is object)
            {
                _sensor.Dispose();
                _sensor = null;
            }
        }
    }
}
