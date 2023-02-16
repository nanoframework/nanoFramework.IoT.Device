//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//
#define ESP32

using System.Device.I2c;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Iot.Device.Adxl343Lib;
#if ESP32
using nanoFramework.Hardware.Esp32;
#endif

namespace Adxl343Samples
{
    public class Program
    {
        private const int Esp32DataPin = 22;
        private const int Esp32ClockPin = 20;
        private const int I2cBusId = 1;
        private const int I2cAddr = 0x53;

        private static I2cDevice _i2c;
        public static void Main()
        {
            Debug.WriteLine("Hello from ADXL343!");

            //////////////////////////////////////////////////////////////////////
            // when not connecting to an ESP32 device, there is not need to configure the GPIOs
            // used for the bus
#if ESP32
                Configuration.SetPinFunction(Esp32DataPin, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(Esp32ClockPin, DeviceFunction.I2C1_CLOCK);
#endif
            // Make sure as well you are using the right chip select

            _i2c = new(new I2cConnectionSettings(I2cBusId, I2cAddr));

            Adxl343 sensor = new Adxl343(_i2c, GravityRange.Range16);

            Debug.WriteLine("Testing Vector...");

            Vector3 v = new Vector3();

            while (true)
            {
                if (sensor.TryGetAcceleration(ref v))
                {
                    Debug.WriteLine("Get Vector Successful");
                    Debug.WriteLine($"X = 0x{v.X}, Y = 0x{v.Y}, Z = 0x{v.Z}");
                }
                Thread.Sleep(1000);
            }
        }
    }
}
