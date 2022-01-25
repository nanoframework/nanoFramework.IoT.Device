// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ms5611;
using nanoFramework.Hardware.Esp32;
using UnitsNet;
using UnitsNet.Units;

namespace Iot.Device.Ms5611.Sample
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello MS5611!");
          
            //////////////////////////////////////////////////////////////////////
            // when connecting to an ESP32 device, need to configure the I2C GPIOs
            // used for the bus
            // Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
            // Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);

            //Czestochowa, Poland, 3rd floor
            double altitude = 250;
            // bus id on the MCU
            const int busId = 1;
            I2cConnectionSettings i2cSettings = new(busId, Ms5611.DefaultI2cAddress);
            using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
            var sensor = new Ms5611(i2cDevice, Sampling.Standard);

            while (true)
            {
                var temperature = sensor.ReadTemperature();
                var pressure = sensor.ReadSeaLevelPressure(Length.From(altitude, LengthUnit.Meter));
                Debug.WriteLine($"Temperature: {temperature.DegreesCelsius.ToString("F")}\u00B0C, Pressure: {pressure.Hectopascals.ToString("F")}hPa");
                Thread.Sleep(5000);
            }
        }
    }
}
