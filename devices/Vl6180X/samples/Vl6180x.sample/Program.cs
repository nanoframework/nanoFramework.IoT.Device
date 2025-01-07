using Iot.Device.VL6180X;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace Vl6180x.sample
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello VL6180X!");
            // when connecting to an ESP32 device, need to configure the I2C GPIOs used for the bus
            Configuration.SetPinFunction(11, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(10, DeviceFunction.I2C1_CLOCK);

            using VL6180X sensor = new(I2cDevice.Create(new I2cConnectionSettings(1, VL6180X.DefaultI2cAddress)));
            sensor.Init();
            while (true)
            {
                var distance = sensor.ReadRange();
                Console.WriteLine($"Distance: {distance.Centimeters} cm.");
                Thread.Sleep(500);
            }
        }
    }
}
