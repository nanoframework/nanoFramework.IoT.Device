// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Hardware.Esp32.Rmt;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Represents WS2815B LED driver.
    /// </summary>
    public class Ws2812c : Ws28xx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ws2812c" /> class.
        /// </summary>
        /// <remarks>In contrast to <see cref="Ws2812b"/> this constructor changes the order of the color values.</remarks>
        /// <param name="gpioPin">The GPIO pin used for communication with the LED driver.</param>
        /// <param name="width">Width of the screen or LED strip.</param>
        /// <param name="height">Height of the screen or LED strip. Defaults to 1 (LED strip).</param>
        public Ws2812c(int gpioPin, int width, int height = 1)
            : base(gpioPin, new BitmapImageWs2808Grb(width, height))
        {
            ClockDivider = 2;
            ClockDivider = 2;
            OnePulse = new RmtCommand(52, true, 52, false);
            ZeroPulse = new RmtCommand(14, true, 52, false);
            ResetCommand = new RmtCommand(1400, false, 1400, false);
        }
    }
}