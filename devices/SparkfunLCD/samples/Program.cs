// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.SparkfunLCD.sample
{
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

            using (var lcd = new SparkfunLCD(SparkfunLCD.DISPLAYSIZE.SIZE20X4, Gpio.IO23, Gpio.IO22))
            {
                lcd.CursorState(false);
                lcd.SetBacklight(0, 255, 0);
                lcd.SetContrast(4);
                lcd.ClearScreen();
                lcd.DisplayState(false);
                lcd.Write(0, 0, "SparkFun 20x4 SerLCD");
                lcd.Write(0, 1, "P/N# LCD-16398");
                lcd.Write(0, 3, "Hello!!!");
                lcd.DisplayState(true);
            }

            ////Sleep.StartDeepSleep();
            while (true) { Thread.Sleep(100); }

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
