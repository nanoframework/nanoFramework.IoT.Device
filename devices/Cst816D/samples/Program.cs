// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Hardware.Esp32;
using nanoFramework.UI;

namespace Iot.Device.Cst816D.Samples
{
    /// <summary>
    /// Reads the touch panel of the Waveshare ESP32-S3-Touch-LCD-2.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The application entry point.
        /// </summary>
        public static void Main()
        {
            Configuration.SetPinFunction(48, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(47, DeviceFunction.I2C1_CLOCK);

            I2cConnectionSettings settings = new I2cConnectionSettings(1, Cst816D.DefaultI2cAddress);
            using Cst816D touch = new Cst816D(new I2cDevice(settings), 240, 320, DisplayOrientation.Portrait);
            Debug.WriteLine($"CST816D chip ID: 0x{touch.ChipId:X2} (expected 0xB6)");

            bool wasTouched = false;
            while (true)
            {
                Point point = touch.GetPoint();
                if (point.IsTouched || wasTouched)
                {
                    Debug.WriteLine($"Touched: {point.IsTouched}, X: {point.X}, Y: {point.Y}, Event: {point.Event}");
                }

                wasTouched = point.IsTouched;
                Thread.Sleep(20);
            }
        }
    }
}
