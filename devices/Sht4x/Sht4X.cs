// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Sht4x
{
    /// <summary>
    /// Humidity and Temperature Sensor Sht4X.
    /// </summary>
    public class Sht4X : IDisposable
    {
        /// <summary>
        /// Default I2C address of the Sht4x sensor.
        /// </summary>
        public const byte DefaultAddress = 0x44;

        /// <summary>
        /// Represents an I2C device used for communication with the Sht4X sensor.
        /// </summary>
        private I2cDevice _i2CDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sht4X" /> class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication.</param>
        public Sht4X(I2cDevice i2CDevice)
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
        /// Reads the temperature and humidity data from the sensor.
        /// </summary>
        /// <param name="precision">The measurement mode to use.</param>
        /// <returns>The sensor data.</returns>
        /// <exception cref="Exception">Thrown if there is an error in the I2C communication.</exception>
        /// <exception cref="InvalidCrcException">Thrown if invalid data is returned from device.</exception>
        public Sht4XSensorData ReadData(MeasurementMode precision)
        {
            var dataToWrite = (byte)precision;
            var delay = GetDelay(precision);
            var result = _i2CDevice.WriteByte(dataToWrite);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            Thread.Sleep(delay);

            var readBuffer = new byte[6];
            _i2CDevice.Read(readBuffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new Exception();
            }

            // Humidity and temperature data will always be transmitted the following way: First temperature signal (2 * 8-bit data + 8-
            // bit CRC), second humidity signal(2 * 8 - bit data + 8 - bit CRC). The serial number is transmitted as two 16 - bit words.
            ValidateCrc8(new[] { readBuffer[0], readBuffer[1] }, readBuffer[2]);
            ValidateCrc8(new[] { readBuffer[3], readBuffer[4] }, readBuffer[5]);

            var temperatureTicks = (readBuffer[0] * 256) + readBuffer[1];
            var humidityTicks = (readBuffer[3] * 256) + readBuffer[4];
            var temperature = -45 + (175 * temperatureTicks / 65535.0f);
            var humidity = -6 + (125 * humidityTicks / 65535.0f);

            return new Sht4XSensorData(
                Temperature.FromDegreesCelsius(temperature),
                RelativeHumidity.FromPercent(humidity));
        }

        private static void ValidateCrc8(byte[] buffer, byte dataCrc)
        {
            byte crc = 0xFF;
            foreach (byte b in buffer)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ 0x31);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            var resultCrc = (byte)(crc & 0xFF);
            if (dataCrc != resultCrc)
            {
                throw new InvalidCrcException();
            }
        }

        private static ushort GetDelay(MeasurementMode precision)
        {
            switch (precision)
            {
                case MeasurementMode.NoHeaterHighPrecision:
                    return 10;
                case MeasurementMode.NoHeaterMediumPrecision:
                    return 5;
                case MeasurementMode.NoHeaterLowPrecision:
                    return 2;
                case MeasurementMode.HighHeat1S:
                    return 1100;
                case MeasurementMode.HighHeat100Ms:
                    return 100;
                case MeasurementMode.MediumHeat1S:
                    return 1100;
                case MeasurementMode.MediumHeat100Ms:
                    return 100;
                case MeasurementMode.LowHeat1S:
                    return 1100;
                case MeasurementMode.LowHeat100Ms:
                    return 100;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
