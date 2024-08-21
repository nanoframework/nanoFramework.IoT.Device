// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Sdc4x
{
    /// <summary>
    /// Humidity and Temperature Sensor Sdc4x.
    /// </summary>
    public class Sdc4x : IDisposable
    {
        private const byte CRC8POLYNOMIAL = 0x31;
        private const byte CRC8INIT = 0xFF;

        /// <summary>
        /// Default I2C address of the Sdc4x sensor.
        /// </summary>
        public const byte I2cDefaultAddress = 0x62;

        /// <summary>
        /// Represents an I2C device used for communication with the Sdc4x sensor.
        /// </summary>
        private I2cDevice _i2CDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sdc4x" /> class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication.</param>
        public Sdc4x(I2cDevice i2CDevice)
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
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            var buffer = new byte[9];
            result = _i2CDevice.Read(buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            var serial1 = (ushort)((buffer[0] << 8) | buffer[1]);
            var serial2 = (ushort)((buffer[2] << 8) | buffer[3]);
            var serial3 = (ushort)((buffer[4] << 8) | buffer[5]);
            return $"0x{serial1:X4}{serial2:X4}{serial3:X4}";
        }

        /// <summary>
        /// Starts the periodic measurment.
        /// </summary>
        public void StartPeriodicMeasurement()
        {
            var result = _i2CDevice.Write(new byte[] { 0x21, 0xB1 });
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Reads the data. 
        /// </summary>
        /// <returns>Sensor data.</returns>
        public Sdc4xSensorData ReadData()
        {
            var result = _i2CDevice.Write(new byte[] { 0xEC, 0x05 });
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            Thread.Sleep(1);
            var buffer = new byte[9];
            result = _i2CDevice.Read(buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            var co2 = (ushort)((buffer[0] << 8) | buffer[1]);
            var tempRaw = (ushort)((buffer[3] << 8) | buffer[4]);
            var humRaw = (ushort)((buffer[6] << 8) | buffer[7]);

            var temp = (tempRaw * 175.0 / 65535.0) - 45.0;
            var hum = humRaw * 100 / 65535.0;
            return new Sdc4xSensorData(Temperature.FromDegreesCelsius(temp), RelativeHumidity.FromPercent(hum), co2);
        }

        /// <summary>
        /// Stops the periodic measurment.
        /// </summary>
        public void StopPeriodicMeasurement()
        {
            var result = _i2CDevice.Write(new byte[] { 0x3F, 0x86 });
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Gets the temperature offset of sensor.
        /// </summary>
        /// <returns>Temperature offset.</returns>
        public Temperature GetTemperatureOffset()
        {
            var result = _i2CDevice.Write(new byte[] { 0x23, 0x18 });
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            Thread.Sleep(1);
            var buffer = new byte[3];
            result = _i2CDevice.Read(buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            var tempRaw = (ushort)((buffer[0] << 8) | buffer[1]);
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
            buffer[2] = (byte)((tOffset & 0xFF00) >> 8);
            buffer[3] = (byte)(tOffset & 0x00FF);
            buffer[4] = crc;

            var result = _i2CDevice.Write(buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }
        }

        private static byte GenerateCRC(byte[] data)
        {
            byte crc = CRC8INIT;

            foreach (var currentByte in data)
            {
                crc ^= currentByte;
                for (int crcBit = 0; crcBit < 8; crcBit++)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ CRC8POLYNOMIAL);
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
            var highByte = (byte)((data >> 8) & 0xFF);
            var lowByte = (byte)(data & 0xFF);
            return GenerateCRC(new byte[] { highByte, lowByte });
        }
    }
}
