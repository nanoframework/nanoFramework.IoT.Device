// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;

namespace Iot.Device.Hdc1080.Sample
{
    public class Program
    {
        public static void Main()
        {
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
            Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
            I2cConnectionSettings settings = new I2cConnectionSettings(1, Hdc1080.DefaultI2cAddress, I2cBusSpeed.FastMode);

            var tempSensor = new Hdc1080(I2cDevice.Create(settings));

            Debug.WriteLine($"SN: {tempSensor.SerialNumber}\r\nDeviceId: {tempSensor.DeviceId}\r\nManufacturerId: {tempSensor.ManufacturerId}");
            while (true)
            {
                var temperature = tempSensor.ReadTemperature();
                Thread.Sleep(20);
                var humidity = tempSensor.ReadHumidity();
                Debug.WriteLine($"Temperature: {temperature.DegreesCelsius.ToString("F")}\u00B0C Humidity: {humidity.Percent.ToString("F")}%");
                Thread.Sleep(5000);
            }
        }
    }
}
