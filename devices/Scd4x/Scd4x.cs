// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Scd4x
{
    /// <summary>
    /// Humidity and Temperature Sensor Scd4x.
    /// </summary>
    public class Scd4x : IDisposable
    {
        private const byte Crc8PolyNominal = 0x31;
        private const byte Crc8 = 0xFF;

        /// <summary>
        /// Default I2C address of the Scd4x sensor.
        /// </summary>
        public const byte I2cDefaultAddress = 0x62;

        /// <summary>
        /// Represents an I2C device used for communication with the Scd4x sensor.
        /// </summary>
        private I2cDevice _i2CDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scd4x" /> class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication.</param>
        public Scd4x(I2cDevice i2CDevice)
        {
            _i2CDevice = i2CDevice ?? throw new ArgumentNullException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2CDevice?.Dispose();
            _i2CDevice = null;
        }

        /// <summary>
        /// Retrieves the serial number from the SDC device.
        /// </summary>
        /// <returns>Serial number.</returns>
        public string GetSerialNumber()
        {
            var result = _i2CDevice.Write(new byte[] { 0x36, 0x82 });
            CheckI2cResultOrThrow(result);

            var buffer = new byte[9];
            result = _i2CDevice.Read(buffer);
            CheckI2cResultOrThrow(result);

            var serial1 = ReadUInt16BigEndian(buffer, 0);
            var serial2 = ReadUInt16BigEndian(buffer, 2);
            var serial3 = ReadUInt16BigEndian(buffer, 4);
            return $"0x{serial1:X4}{serial2:X4}{serial3:X4}";
        }

        /// <summary>
        /// Starts the periodic measurment.
        /// </summary>
        public void StartPeriodicMeasurement()
        {
            var result = _i2CDevice.Write(new byte[] { 0x21, 0xB1 });
            CheckI2cResultOrThrow(result);
        }

        /// <summary>
        /// Reads the data. 
        /// </summary>
        /// <returns>Sensor data.</returns>
        public Scd4xSensorData ReadData()
        {
            var result = _i2CDevice.Write(new byte[] { 0xEC, 0x05 });
            CheckI2cResultOrThrow(result);

            Thread.Sleep(1);
            var buffer = new byte[9];
            result = _i2CDevice.Read(buffer);
            CheckI2cResultOrThrow(result);

            var co2 = ReadUInt16BigEndian(buffer, 0);
            var tempRaw = ReadUInt16BigEndian(buffer, 3);
            var humRaw = ReadUInt16BigEndian(buffer, 6);

            var temp = (tempRaw * 175.0 / 65535.0) - 45.0;
            var hum = humRaw * 100 / 65535.0;
            return new Scd4xSensorData(Temperature.FromDegreesCelsius(temp), RelativeHumidity.FromPercent(hum), VolumeConcentration.FromPartsPerMillion(co2));
        }

        /// <summary>
        /// Stops the periodic measurment.
        /// </summary>
        public void StopPeriodicMeasurement()
        {
            var result = _i2CDevice.Write(new byte[] { 0x3F, 0x86 });
            CheckI2cResultOrThrow(result);
        }

        /// <summary>
        /// Gets the temperature offset of sensor.
        /// </summary>
        /// <returns>Temperature offset.</returns>
        public Temperature GetTemperatureOffset()
        {
            var result = _i2CDevice.Write(new byte[] { 0x23, 0x18 });
            CheckI2cResultOrThrow(result);

            Thread.Sleep(1);
            var buffer = new byte[3];
            result = _i2CDevice.Read(buffer);
            CheckI2cResultOrThrow(result);

            var tempRaw = ReadUInt16BigEndian(buffer, 0);
            var temp = tempRaw * 175.0 / 65535.0;
            return Temperature.FromDegreesCelsius(temp);
        }

        /// <summary>
        /// Sets the temperature offset. Doesn't affects CO2 measurments.
        /// </summary>
        /// <param name="offset">New offset.</param>
        public void SetTemperatureOffset(Temperature offset)
        {
            var asTicks = (offset.DegreesCelsius * 65536.0 / 175.0) + 0.5f;
            var tOffset = (ushort)asTicks;
            var crc = GenerateCRC(tOffset);
            var buffer = new byte[5];
            buffer[0] = 0x24;
            buffer[1] = 0x1D;
            buffer[2] = (byte)(tOffset >> 8);
            buffer[3] = (byte)(tOffset & 0x00FF);
            buffer[4] = crc;

            var result = _i2CDevice.Write(buffer);
            CheckI2cResultOrThrow(result);
        }

        /// <summary>
        /// Gets the status of data.
        /// </summary>
        /// <returns>True if data is ready to read.</returns>
        public bool IsDataReady()
        {
            var result = _i2CDevice.Write(new byte[] { 0xE4, 0xB8 });
            CheckI2cResultOrThrow(result);

            Thread.Sleep(1);
            var buffer = new byte[3];
            result = _i2CDevice.Read(buffer);
            CheckI2cResultOrThrow(result);
            
            var word = ReadUInt16BigEndian(buffer, 0);
            return (word & 0x07FF) != 0;
        }

        private static byte GenerateCRC(byte[] data)
        {
            byte crc = Crc8;

            foreach (var currentByte in data)
            {
                crc ^= currentByte;
                for (int crcBit = 0; crcBit < 8; crcBit++)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ Crc8PolyNominal);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }

            return crc;
        }

        private static byte GenerateCRC(ushort data)
        {
            var highByte = (byte)(data >> 8);
            var lowByte = (byte)(data & 0xFF);
            return GenerateCRC(new byte[] { highByte, lowByte });
        }

        private void CheckI2cResultOrThrow(I2cTransferResult status)
        {
            if (status.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }
        }

        private ushort ReadUInt16BigEndian(byte[] buffer, int startIndex)
        {
            return (ushort)((buffer[startIndex] << 8) | buffer[startIndex + 1]);
        }
    }
}
