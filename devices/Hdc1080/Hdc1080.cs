// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Device.I2c;
using System.Device.Model;
using UnitsNet;

namespace Iot.Device.Hdc1080
{
    /// <summary>
    /// Temperature and Humidity sensor Hdc1080
    /// </summary>
    [Interface("Hdc1080 - temperature and humidity sensor")]
    public class Hdc1080 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private int _wakeUpTimeMilliseconds = 20;

        /// <summary>
        /// Default I2C address
        /// </summary>
        public const byte DefaultI2cAddress = 0x40;

        /// <summary>
        /// Constructs Hdc1080 instance with high temperature and humidity sampling. Measurement mode set by default to humidity and temperature.
        /// </summary>
        /// <param name="i2cDevice">I2C device used to communicate with the device</param>
        /// <param name="humidityResolution">Humidity sampling resolution</param>
        /// <param name="temperatureResolution">Temperature sampling resolution</param>
        public Hdc1080(I2cDevice i2cDevice, HumidityResolution humidityResolution = HumidityResolution.High, TemperatureResolution temperatureResolution = TemperatureResolution.High)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            // According to data sheet sensor needs at least 15ms after wake up to be ready.
            Thread.Sleep(_wakeUpTimeMilliseconds);
            TemperatureResolution = temperatureResolution;
            HumidityResolution = humidityResolution;
        }

        /// <summary>
        /// Reads temperature, check data sheet, page 14, 8.6.1 section
        /// </summary>
        /// <returns>
        /// Temperature
        /// </returns>
        [Telemetry("Temperature")]
        public Temperature ReadTemperature() => Temperature.FromDegreesCelsius((ReadData(RegisterAddress.TemperatureMeasurement) * 165 / 65536f) - 40);

        /// <summary>
        /// Reads relative humidity, check data sheet, page 14, 8.6.2 section
        /// </summary>
        /// <returns>
        /// Relative humidity
        /// </returns>
        [Telemetry("Humidity")]
        public RelativeHumidity ReadHumidity() => RelativeHumidity.FromPercent(ReadData(RegisterAddress.HumidityMeasurement) * 100 / 65536);

        /// <summary>
        /// Reads device id, check data sheet, page 14, 8.6.6 section
        /// </summary>
        /// <returns>
        /// Device id
        /// </returns>
        [Property("DeviceId")]
        public string DeviceId => ReadData(RegisterAddress.DeviceId).ToString();

        /// <summary>
        /// Reads manufacturer id, check data sheet, page 14, 8.6.5 section
        /// </summary>
        /// <returns>
        /// Manufacturer id
        /// </returns>
        [Property("ManufacturerId")]
        public string ManufacturerId => ReadData(RegisterAddress.ManufacturerId).ToString();

        /// <summary>
        /// Reads serial number, check data sheet, page 14, 8.6.4 section
        /// </summary>
        /// <returns>
        /// Serial number
        /// </returns>
        [Property("SerialNumber")]
        public string SerialNumber => $"{ReadData(RegisterAddress.SerialIdFirstByte)}{ReadData(RegisterAddress.SerialIdSecondByte)}{ReadData(RegisterAddress.SerialIdThirdByte)}";

        /// <summary>
        /// Sets or gets humidity resolution
        /// </summary>
        [Property("HumidityResolution")]
        public HumidityResolution HumidityResolution
        {
            get
            {
                ConfigurationRegister register = (ConfigurationRegister)ReadRegister();
                return (HumidityResolution)register.HumidityMeasurementResolution;
            }

            set
            {
                ConfigurationRegister register = (ConfigurationRegister)ReadRegister();
                register.HumidityMeasurementResolution = (byte)value;

                WriteRegister(register);
            }
        }

        /// <summary>
        /// Sets or gets temperature resolution
        /// </summary>
        [Property("TemperatureResolution")]
        public TemperatureResolution TemperatureResolution
        {
            get
            {
                ConfigurationRegister register = (ConfigurationRegister)ReadRegister();
                return (TemperatureResolution)register.TemperatureMeasurementResolution;
            }

            set
            {
                ConfigurationRegister register = (ConfigurationRegister)ReadRegister();
                register.TemperatureMeasurementResolution = (byte)value;

                WriteRegister(register);
            }
        }

        /// <summary>
        /// Runs heater for given time, check data sheet, page 9, 8.3.3 section
        /// </summary>
        /// <param name="heatingTime">Heating time. Must be greater than 1 second</param>
        public void HeatUp(TimeSpan heatingTime)
        {
            if (heatingTime.Seconds < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(heatingTime), "Heating time can't be lower than 1 second.");
            }

            SetHeater(true);
            for (int i = 0; i < heatingTime.Seconds; i++)
            {
                _i2cDevice.Write(new byte[] { (byte)RegisterAddress.TemperatureMeasurement });
                // wait SCL free
                Thread.Sleep(_wakeUpTimeMilliseconds);
                _i2cDevice.Read(new byte[4]);
                Thread.Sleep(1000 - _wakeUpTimeMilliseconds);
            }

            SetHeater(false);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        private long ReadData(RegisterAddress address)
        {
            byte[] resultData = new byte[2];
            _i2cDevice.Write(new byte[] { (byte)address });
            // wait SCL free
            Thread.Sleep(_wakeUpTimeMilliseconds);
            _i2cDevice.Read(resultData);
            return resultData[0] << 8 | resultData[1];
        }

        private void WriteRegister(ConfigurationRegister register)
        {
            _i2cDevice.Write(new byte[] { (byte)RegisterAddress.Configuration, register.GetData(), (byte)RegisterAddress.TemperatureMeasurement });
            // wait SCL free
            Thread.Sleep(_wakeUpTimeMilliseconds);
        }

        private ConfigurationRegister ReadRegister()
        {
            ConfigurationRegister register = new ConfigurationRegister();
            register.SetData((byte)(ReadData(RegisterAddress.Configuration) >> 8));
            return register;
        }

        private void SetHeater(bool isOn)
        {
            if (isOn)
            {
                ConfigurationRegister register = (ConfigurationRegister)ReadRegister();
                register.Heater = true;
                register.SeparateReadings = false;
                WriteRegister(register);
            }
            else
            {
                ConfigurationRegister register = (ConfigurationRegister)ReadRegister();
                register.Heater = false;
                register.SeparateReadings = true;
                WriteRegister(register);
            }
        }
    }
}