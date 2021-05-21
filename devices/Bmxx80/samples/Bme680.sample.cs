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

            // The I2C bus ID on the Raspberry Pi 3.
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
                    Debug.WriteLine($"Gas resistance: {readResult.GasResistance.Ohms:0.##}Ohm");
                    Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius:0.#}\u00B0C");
                    Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals:0.##}hPa");
                    Debug.WriteLine($"Relative humidity: {readResult.Humidity.Percent:0.#}%");

                    if (!readResult.Temperature.Equals(null) && !readResult.Pressure.Equals(null))
                    {
                        var altValue = WeatherHelper.CalculateAltitude(readResult.Pressure, defaultSeaLevelPressure, readResult.Temperature);
                        Debug.WriteLine($"Altitude: {altValue.Meters:0.##}m");
                    }

                    if (!readResult.Temperature.Equals(null) && !readResult.Humidity.Equals(null))
                    {
                        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                        Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(readResult.Temperature, readResult. Humidity).DegreesCelsius:0.#}\u00B0C");
                        Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(readResult.Temperature, readResult.Humidity).DegreesCelsius:0.#}\u00B0C");
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
                    Debug.WriteLine($"Gas resistance: {readResult.GasResistance.Ohms:0.##}Ohm");
                    Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius:0.#}\u00B0C");
                    Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals:0.##}hPa");
                    Debug.WriteLine($"Relative humidity: {readResult.Humidity.Percent:0.#}%");

                    if (!readResult.Temperature.Equals(null) && !readResult.Pressure.Equals(null))
                    {
                        var altValue = WeatherHelper.CalculateAltitude(readResult.Pressure, defaultSeaLevelPressure, readResult.Temperature);
                        Debug.WriteLine($"Altitude: {altValue.Meters:0.##}m");
                    }

                    if (!readResult.Temperature.Equals(null) && !readResult.Humidity.Equals(null))
                    {
                        // WeatherHelper supports more calculations, such as saturated vapor pressure, actual vapor pressure and absolute humidity.
                        Debug.WriteLine($"Heat index: {WeatherHelper.CalculateHeatIndex(readResult.Temperature, readResult.Humidity).DegreesCelsius:0.#}\u00B0C");
                        Debug.WriteLine($"Dew point: {WeatherHelper.CalculateDewPoint(readResult.Temperature, readResult.Humidity).DegreesCelsius:0.#}\u00B0C");
                    }

                    Thread.Sleep(1000);
                }
            }

        }
    }
}