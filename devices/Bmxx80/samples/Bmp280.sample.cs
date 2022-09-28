// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Common;
using UnitsNet;
//using nanoFramework.Hardware.Esp32;

namespace Iot.Device.Bmxx80.sample
{
    public class Bmp280_sample
    {
        public static void RunSample()
        {
            Debug.WriteLine("Hello Bmp280!");

            Length stationHeight = Length.FromMeters(640); // Elevation of the sensor

            // check the I2C bus that you're connecting on your .NET nanoFramework device
            const int busId = 1;

            //////////////////////////////////////////////////////////////////////
            // when connecting to an ESP32 device, need to configure the I2C GPIOs
            // used for the bus
            //Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
            //Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

            ////////////////////////////////////////////////////////////////////////////////////////
            // Set this to the current sea level pressure in the area for correct altitude readings.
            // Default is the agreed MSL value.
            Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

            I2cConnectionSettings i2cSettings = new(busId, Bmp280.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            using var i2CBmp280 = new Bmp280(i2cDevice);

            while (true)
            {
                // set higher sampling
                i2CBmp280.TemperatureSampling = Sampling.LowPower;
                i2CBmp280.PressureSampling = Sampling.UltraHighResolution;

                // Perform a synchronous measurement
                var readResult = i2CBmp280.Read();

                // Print out the measured data
                if (readResult.TemperatureIsValid)
                {
                    Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius}\u00B0C");
                }
                if (readResult.PressureIsValid)
                {
                    Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals}hPa");
                }

                // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
                // double altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                i2CBmp280.TryReadAltitude(out var altValue);

                Debug.WriteLine($"Calculated Altitude: {altValue.Meters}m");
                Thread.Sleep(1000);

                // change sampling rate
                i2CBmp280.TemperatureSampling = Sampling.UltraHighResolution;
                i2CBmp280.PressureSampling = Sampling.UltraLowPower;
                i2CBmp280.FilterMode = Bmx280FilteringMode.X4;

                // Perform an asynchronous measurement
                readResult = i2CBmp280.Read();

                // Print out the measured data
                if (readResult.TemperatureIsValid)
                {
                    Debug.WriteLine($"Temperature: {readResult.Temperature.DegreesCelsius}\u00B0C");
                }
                if (readResult.PressureIsValid)
                {
                    Debug.WriteLine($"Pressure: {readResult.Pressure.Hectopascals}hPa");
                }

                // This time use altitude calculation
                if (readResult.TemperatureIsValid && readResult.PressureIsValid)
                {
                    altValue = WeatherHelper.CalculateAltitude((Pressure)readResult.Pressure, defaultSeaLevelPressure, (Temperature)readResult.Temperature);
                    Debug.WriteLine($"Calculated Altitude: {altValue.Meters}m");
                }

                // Calculate the barometric (corrected) pressure for the local position.
                // Change the stationHeight value above to get a correct reading, but do not be tempted to insert
                // the value obtained from the formula above. Since that estimates the altitude based on pressure,
                // using that altitude to correct the pressure won't work.
                if (readResult.TemperatureIsValid && readResult.PressureIsValid)
                {
                    var correctedPressure = WeatherHelper.CalculateBarometricPressure((Pressure)readResult.Pressure, (Temperature)readResult.Temperature, stationHeight);
                    Debug.WriteLine($"Pressure corrected for altitude {stationHeight}m (with average humidity): {correctedPressure.Hectopascals} hPa");
                }

                Thread.Sleep(5000);
            }
        }
    }
}