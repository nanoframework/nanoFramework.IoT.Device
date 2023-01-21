// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Hardware.Esp32.Rmt;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Represents WS2812B LED driver.
    /// </summary>
    public class Ws2812b : Ws28xx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ws2812b" /> class.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin used for communication with the LED driver.</param>
        /// <param name="width">Width of the screen or LED strip.</param>
        /// <param name="height">Height of the screen or LED strip. Defaults to 1 (LED strip).</param>
        public Ws2812b(int gpioPin, int width, int height = 1)
            : base(gpioPin, new BitmapImageNeo3(width, height))
        {
            ClockDivider = 2;
            OnePulse = new RmtCommand(32, true, 18, false);
            ZeroPulse = new RmtCommand(16, true, 34, false);
            ResetCommand = new RmtCommand(2000, false, 2000, false);
        }
    }
}
