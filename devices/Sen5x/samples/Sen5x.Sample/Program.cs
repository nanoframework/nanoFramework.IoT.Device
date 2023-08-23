// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Sen5x;
using nanoFramework.Hardware.Esp32;

namespace Sen5x.Sample
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello world!");

            Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
            var settings = new I2cConnectionSettings(1, Sen5xSensor.DefaultI2cAddress, Sen5xSensor.DefaultI2cBusSpeed);
            var i2c = new I2cDevice(settings);
            var sensor = new Sen5xSensor(i2c);

            sensor.StartMeasurement();
            while (true)
            {
                if (sensor.ReadDataReadyFlag())
                {
                    var measurement = sensor.ReadMeasurement();
                    Debug.WriteLine(measurement.ToString());
                }
                Thread.Sleep(1000);
            }
        }
    }
}
