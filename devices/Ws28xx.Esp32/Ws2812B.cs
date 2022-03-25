// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Represents WS2812B LED driver
    /// </summary>
    public class Ws2812b : Ws28xx
    {
        /// <summary>
        /// Constructs Ws2812b instance
        /// </summary>
        /// <param name="gpioPin">The GPIO pin used for communication with the LED driver</param>
        /// <param name="width">Width of the screen or LED strip</param>
        /// <param name="height">Height of the screen or LED strip. Defaults to 1 (LED strip).</param>
        public Ws2812b(int gpioPin, int width, int height = 1)
            : base(gpioPin, new BitmapImageNeo3(width, height))
        {
            ClockDivider = 2;
            OnePulse = new(52, true, 52, false);
            ZeroPulse = new(14, true, 52, false);
            ResetCommand = new(1400, false, 1400, false);
        }
    }
}
