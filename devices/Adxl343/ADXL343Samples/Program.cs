//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using ADXL343;

namespace ADXL343Samples
{
    public class Program
    {
        private const bool useESP32 = true;
        private const int ESP32DataPin = 22;
        private const int ESP32ClockPin = 20;
        private const int i2cBusId = 1;
        private const int i2cAddr = 0x53;

        private static I2cDevice i2c;
        public static void Main()
        {
            Debug.WriteLine("Hello from ADXL343!");

            //////////////////////////////////////////////////////////////////////
            // when not connecting to an ESP32 device, there is not need to configure the GPIOs
            // used for the bus
            if (useESP32)
            {
                Configuration.SetPinFunction(ESP32DataPin, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(ESP32ClockPin, DeviceFunction.I2C1_CLOCK);
            }
            // Make sure as well you are using the right chip select

            i2c = new(new I2cConnectionSettings(i2cBusId, i2cAddr));

            ADXL343.ADXL343 sensor = new ADXL343.ADXL343(i2c, GravityRange.Range16);

            Debug.WriteLine("Testing Vector...");

            Vector3 v = new Vector3();

            while (true)
            {
                if (sensor.GetAcceleration(ref v))
                {
                    Debug.WriteLine("Get Vector Successful");
                    Debug.WriteLine($"X = 0x{v.X}, Y = 0x{v.Y}, Z = 0x{v.Z}");
                }
                Thread.Sleep(1000);
            }
        }
    }
}
