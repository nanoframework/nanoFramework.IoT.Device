// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Hardware.Esp32.Rmt;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Represents WS2815B LED driver.
    /// </summary>
    public class Ws2815b : Ws28xx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ws2815b" /> class.
        /// </summary>
        /// <remarks>In contrast to <see cref="Ws2812b"/> this constructor changes the order of the color values.</remarks>
        /// <param name="gpioPin">The GPIO pin used for communication with the LED driver.</param>
        /// <param name="width">Width of the screen or LED strip.</param>
        /// <param name="height">Height of the screen or LED strip. Defaults to 1 (LED strip).</param>
        public Ws2815b(int gpioPin, int width, int height = 1)
            : base(gpioPin, new BitmapImageNeo3Rgb(width, height), 6, 3, 3, 6, 2900)
        {
            // 100ns tick
            // T0l(6) = 600ns  
            // T0h(3) = 300ns
            // T1l(3) = 300ns
            // T1h(6) = 600ns
            // reset(2900) = 290us (280us > )
        }
    }
}
