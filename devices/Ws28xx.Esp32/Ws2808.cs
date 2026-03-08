// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Hardware.Esp32.Rmt;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Represents WS2808 LED driver.
    /// </summary>
    public class Ws2808 : Ws28xx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ws2808" /> class.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin used for communication with the LED driver.</param>
        /// <param name="width">Width of the screen or LED strip.</param>
        /// <param name="height">Height of the screen or LED strip. Defaults to 1 (LED strip).</param>
        /// <param name="rmtChannel">
        /// The RMT channel number to use. Valid values are 0 to 7 (inclusive) for explicit selection,
        /// or -1 to automatically select an available channel (default).
        /// </param>
        public Ws2808(int gpioPin, int width, int height = 1, int rmtChannel = -1)
            : base(gpioPin, new BitmapImageWs2808(width, height), rmtChannel)
        {
            ClockDivider = 2;
            OnePulse = new RmtCommand(52, true, 52, false);
            ZeroPulse = new RmtCommand(14, true, 52, false);
            ResetCommand = new RmtCommand(1400, false, 1400, false);
        }
    }
}
