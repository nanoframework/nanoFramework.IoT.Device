// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Common;
using UnitsNet;

namespace Iot.Device.Bmxx80.sample
{
    public class Bme680_sample
    {
        public static void RunSample()
        {
            Debug.WriteLine("Hello BME680!");

            //////////////////////////////////////////////////////////////////////
            // when connecting to an ESP32 device, need to configure the I2C GPIOs
            // used for the bus
            //Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
            //Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

            // The I2C bus ID on the MCU.
            const int busId = 1;
            // set this to the current sea level pressure in the area for correct altitude readings
            Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

            I2cConnectionSettings i2cSettings = new(busId, Bme680.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

            using Bme680 bme680 = new Bme680(i2cDevice, Temperature.FromDegreesCelsius(20.0));

            while (true)
            {
                // reset will change settings back to default
                bme680.Reset();

                // 10 consecutive measurement with default settings
                for (var i = 0; i < 10; i++)
                {
                    // Perform a synchronous measurement
                    var readResult = bme680.Read();

                    // Print out the measured data
                    if (readResult.GasResistanceIsValid)
                    {
                        Debug.WriteLine($"Gas resistance: {readResult.GasResistance.Ohms}Ohm");
                    }
                    if (readResult.TemperatureIsValid)
                    {
                        Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius}\u00B0C");
                    }
                    if (readResult.PressureIsValid)
                    {
                        Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals}hPa");
                    }
                    if (readResult.HumidityIsValid)
                    {
                        Debug.WriteLine($"Relative humidity: {readResult.Humidity.Percent}%");
                    }

                    if (readResult.TemperatureIsValid && readResult.PressureIsValid)
                    {
                        var altValue = WeatherHelper.CalculateAltitude(readResult.Pressure, defaultSeaLevelPressure, readResult.Temperature);
                        Debug.WriteLine($"Altitude: {altValue.Meters}m");
                    }

                    if (readResult.TemperatureIsValid && readResult.HumidityIsValid)
                    {
                        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                        Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(readResult.Temperature, readResult.Humidity).DegreesCelsius}\u00B0C");
                        Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(readResult.Temperature, readResult.Humidity).DegreesCelsius}\u00B0C");
                    }

                    // when measuring the gas resistance on each cycle it is important to wait a certain interval
                    // because a heating plate is activated which will heat up the sensor without sleep, this can
                    // falsify all readings coming from the sensor
                    Thread.Sleep(1000);
                }

                // change the settings
                bme680.TemperatureSampling = Sampling.HighResolution;
                bme680.HumiditySampling = Sampling.UltraHighResolution;
                bme680.PressureSampling = Sampling.Skipped;

                bme680.ConfigureHeatingProfile(Bme680HeaterProfile.Profile2, Temperature.FromDegreesCelsius(280), Duration.FromMilliseconds(80), Temperature.FromDegreesCelsius(24));
                bme680.HeaterProfile = Bme680HeaterProfile.Profile2;

                // 10 consecutive measurements with custom settings
                for (int i = 0; i < 10; i++)
                {
                    // Perform an asynchronous measurement
                    var readResult = bme680.Read();

                    // Print out the measured data
                    if (readResult.GasResistanceIsValid)
                    {
                        Debug.WriteLine($"Gas resistance: {readResult.GasResistance.Ohms}Ohm");
                    }
                    if (readResult.TemperatureIsValid)
                    {
                        Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius}\u00B0C");
                    }
                    if (readResult.PressureIsValid)
                    {
                        Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals}hPa");
                    }
                    if (readResult.HumidityIsValid)
                    {
                        Debug.WriteLine($"Relative humidity: {readResult.Humidity.Percent}%");
                    }

                    if (readResult.TemperatureIsValid && readResult.PressureIsValid)
                    {
                        var altValue = WeatherHelper.CalculateAltitude(readResult.Pressure, defaultSeaLevelPressure, readResult.Temperature);
                        Debug.WriteLine($"Altitude: {altValue.Meters}m");
                    }

                    if (!readResult.TemperatureIsValid && readResult.HumidityIsValid)
                    {
                        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                        Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(readResult.Temperature, readResult.Humidity).DegreesCelsius}\u00B0C");
                        Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(readResult.Temperature, readResult.Humidity).DegreesCelsius}\u00B0C");
                    }

                    Thread.Sleep(1000);
                }
            }
        }
    }
}