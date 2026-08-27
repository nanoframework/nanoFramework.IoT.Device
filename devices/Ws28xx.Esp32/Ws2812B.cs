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
            : base(gpioPin, new BitmapImageNeo3(width, height), 9, 4, 5, 8, 550)
        {
            // 100ns tick
            // T0l(9) = 900ns  
            // T0h(4)  = 400ns
            // T1l(5) = 500ns
            // T1h(8) = 800ns
            // reset(550) = 55us (50us > )
        }
    }
}
