// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SparkfunLCD.sample
{
    using System;
    using System.Device.I2c;
    using System.Diagnostics;
    using System.Threading;
    using nanoFramework.Hardware.Esp32;
    using Iot.Device.SparkfunLCD;

    /// <summary>
    /// Class containing main executable code
    /// </summary>
    public class Program
    {
        /// <summary>
        /// code executed at device reset
        /// </summary>
        public static void Main()
        {
            Debug.WriteLine("Hello from SparkFun 20x4 SerLCD");

            // configure ESP32 device I2C bus
            {
                // note: actual pin-out is specific to Adafruit Huzzah32 Feather on which code was tested
                int dataPin = Gpio.IO23;
                int clockPin = Gpio.IO22;
                Configuration.SetPinFunction(dataPin, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(clockPin, DeviceFunction.I2C1_CLOCK);
            }

            var settings = new I2cConnectionSettings(busId: 1, deviceAddress: SparkfunLcd.DefaultI2cAddress, busSpeed: I2cBusSpeed.StandardMode);
            using (var i2cDevice = I2cDevice.Create(settings))
            {
                using (var lcd = new SparkfunLcd(i2cDevice, SparkfunLcd.DisplaySizeEnum.Size20x4))
                {
                    lcd.SetBacklight(0, 255, 0);
                    lcd.SetContrast(4);
                    lcd.Clear();
                    lcd.SetDisplayState(false);
                    lcd.Write(0, 0, "SparkFun 20x4 SerLCD");
                    lcd.Write(0, 1, "P/N# LCD-16398");
                    lcd.Write(0, 3, "Hello!!!");
                    lcd.SetDisplayState(true);
                }
            }
        }
    }
}
