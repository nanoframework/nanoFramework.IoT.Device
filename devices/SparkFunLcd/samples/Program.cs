// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SparkFunLcd.Sample
{
    using Iot.Device.SparkFunLcd;
    using nanoFramework.Hardware.Esp32;
    using System.Device.I2c;
    using System.Diagnostics;
    using System.Drawing;

    /// <summary>
    /// Class containing main executable code.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Code executed at device reset.
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

            var settings = new I2cConnectionSettings(busId: 1, deviceAddress: SparkFunLcd.DefaultI2cAddress, busSpeed: I2cBusSpeed.StandardMode);
            using (var i2cDevice = I2cDevice.Create(settings))
            {
                using (var lcd = new SparkFunLcd(i2cDevice, SparkFunLcd.DisplaySizeEnum.Size20x4))
                {
                    lcd.SetBacklight(Color.FromArgb(0, 255, 0));
                    lcd.SetContrast(4);

                    lcd.SetDisplayState(false);
                    lcd.Clear();

                    // demonstrating custom characters
                    {
                        lcd.CreateCustomCharacter(0, new byte[] { 0x0, 0x1b, 0xe, 0x4, 0xe, 0x1b, 0x0, 0x0 }); // define custom character 0x0
                        lcd.CreateCustomCharacter(1, new byte[] { 0x0, 0x1, 0x3, 0x16, 0x1c, 0x8, 0x0, 0x0 }); // define custom character 0x1

                        lcd.SetCursorPosition(0, 3);
                        lcd.Write("custom chars: ");
                        lcd.Write(new char[] { '\x0', '\x1' }); // write custom character 0x0 followed by custom character 0x1
                    }

                    lcd.Write(0, 0, "SparkFun 20x4 SerLCD");
                    lcd.Write(0, 1, "P/N# LCD-16398");

                    lcd.SetDisplayState(true);
                }
            }
        }
    }
}
