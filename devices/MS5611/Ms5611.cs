// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Model;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.MS5611
{
    /// <summary>
    /// MS5611/GY-63 - Temperature and pressure sensor 
    /// </summary>
    [Interface("MS5611/GY-63 - temperature and pressure sensor")]
    public class Ms5611 : IDisposable
    {
        private Sampling _sampling;
        private int _delayForSampling = 0;
        private I2cDevice _i2cDevice;
        private CalibrationData _calibrationData;

        /// <summary>
        /// Default I2C address
        /// </summary>
        public static readonly byte DefaultI2cAddress = 0x77;

        /// <summary>
        /// Alternative I2C address
        /// </summary>
        public static readonly byte AlternativeI2cAddress = 0x76;

        /// <summary>
        /// Constructs MS5611 instance
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        public Ms5611(I2cDevice i2cDevice, Sampling oversampling = Sampling.HighResolution)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _calibrationData = new CalibrationData();
            _i2cDevice.WriteByte((byte)CommandAddresses.Reset);

            Thread.Sleep(100);

            // Read the coefficients table
            _calibrationData.ReadFromDevice(this);
            SetSampling(oversampling);
        }

        /// <summary>
        /// Sets sampling to the given value
        /// </summary>
        /// <param name="sampling">Sampling Mode</param>
        public void SetSampling(Sampling sampling)
        {
            _sampling = sampling;
            _delayForSampling = sampling switch
            {
                Sampling.UltraLowPower => 1,
                Sampling.LowPower => 2,
                Sampling.Standard => 3,
                Sampling.HighResolution => 5,
                Sampling.UltraHighResolution => 10,
                _ => 5
            };
        }

        /// <summary>
        ///  Reads the temperature from the sensor
        /// </summary>
        /// <returns>
        ///  Temperature in degrees celsius
        /// </returns>
        [Telemetry("Temperature")]
        public Temperature ReadTemperature() => Temperature.FromDegreesCelsius(ReadTrueTemperature());

        /// <summary>
        ///  Reads the pressure from the sensor
        /// </summary>
        /// <returns>
        ///  Atmospheric pressure
        /// </returns>
        [Telemetry("Pressure")]
        public Pressure ReadPressure() => Pressure.FromMillibars(ReadTruePressure(compensation: false));

        /// <summary>
        ///  Calculates the pressure at sea level when given a known altitude
        /// </summary>
        /// <param name="altitude" >
        ///  Altitude in meters
        /// </param>
        /// <returns>
        ///  Pressure
        /// </returns>
        public Pressure ReadSeaLevelPressure(Length altitude) => WeatherHelper.CalculateSeaLevelPressure(ReadPressure(), altitude, ReadTemperature());

        /// <summary>
        ///  Reads raw temperature value from the sensor
        /// </summary>
        /// <param name="compensation" >
        ///  Flag indicating if compensation calculation for low temperatures (lower than 20 degrees C) should be done
        /// </param>
        /// <returns>
        ///  Temperature in Celsius
        /// </returns>
        public double ReadTrueTemperature(bool compensation = false)

        {
            long rawTemp = ReadRawData((byte)((byte)CommandAddresses.SamplingRateTemperature + _sampling));
            long diffActualAndReference = rawTemp - _calibrationData.ReferenceTemperature * 256;
            double actualTemperature = 2000 + (double)diffActualAndReference * _calibrationData.TemperatureCoefficientOfTheTemperature / 8388608;
            double compensationValue = 0;
            if (compensation)
            {
                if (actualTemperature < 2000)
                {
                    compensationValue = diffActualAndReference * diffActualAndReference / (2 << 30);
                }
            }
            var temp = actualTemperature - compensationValue;
            return (double)temp / 100;
        }

        internal int ReadRegister(int numberOfByte)
        {
            _i2cDevice.WriteByte((byte)numberOfByte);
            var readData = new byte[2];
            _i2cDevice.Read(new SpanByte(readData));
            int rawData = readData[0] << 8 | readData[1];
            return rawData;
        }

        internal long ReadRawData(byte address)
        {
            _i2cDevice.WriteByte(address);
            Thread.Sleep(_delayForSampling);
            _i2cDevice.WriteByte((byte)CommandAddresses.AdcRead);
            var readData = new byte[3];
            _i2cDevice.Read(new SpanByte(readData));
            long rawData = readData[0] << 16 | readData[1] << 8 | readData[2];
            return rawData;
        }

        /// <summary>
        ///  Reads raw pressure value from the sensor
        /// </summary>
        /// <param name="compensation" >
        ///  Flag indicating if compensation calculation for low temperatures (lower than 20 degrees C) should be done
        /// </param>
        /// <returns>
        ///  Pressure in millibars
        /// </returns>
        public double ReadTruePressure(bool compensation = false)
        {
            long rawPressure = ReadRawData((byte)((byte)CommandAddresses.SamplingRatePressure + _sampling));
            long rawTemp = ReadRawData((byte)((byte)CommandAddresses.SamplingRateTemperature + _sampling));
            long diffActualAndReferenceTemperature = (long)(rawTemp - _calibrationData.ReferenceTemperature * 256);
            long actualTemperature = 2000 + diffActualAndReferenceTemperature * _calibrationData.TemperatureCoefficientOfTheTemperature / 8388608;
            long offset = (long)_calibrationData.PressureOffset * 65536 + _calibrationData.TemperatureCoefficientOfPressureOffset *
                diffActualAndReferenceTemperature / 128;
            long sensitivity = _calibrationData.PressureSensitivity * 32768 + _calibrationData.TemperatureCoefficientOfPressureSensitivity * diffActualAndReferenceTemperature / 256;
            if (compensation)
            {
                CalculateCompensation();
            }

            long pressure = (rawPressure * sensitivity / 2097152 - offset) / 3276800;
            return pressure;

            void CalculateCompensation()
            {
                long compensatedOffset = 0;
                long compensatedSensitivity = 0;

                if (actualTemperature < -1500)
                {
                    compensatedOffset = 7 * (actualTemperature + 1500) * (actualTemperature + 1500);
                    compensatedSensitivity = 7 * (actualTemperature + 1500) * (actualTemperature + 1500) / 2;
                }

                if (actualTemperature < 2000)
                {
                    compensatedOffset = 5 * (actualTemperature - 2000) * (actualTemperature - 2000) / 2;
                    compensatedSensitivity = 5 * (actualTemperature - 2000) * (actualTemperature - 2000) / 4;
                }


                offset = offset - compensatedOffset;
                sensitivity = sensitivity - compensatedSensitivity;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }
    }
}