// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Hardware.Esp32.Rmt;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Represents the SK6812 Driver.
    /// </summary>
    /// <seealso cref="Iot.Device.Ws28xx.Ws28xx" />
    public class Sk6812 : Ws28xx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sk6812"/> class.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin used for communication with the LED driver.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Sk6812(int gpioPin, int width, int height = 1)
            : base(gpioPin, new BitmapImageWs2808Grb(width, height))
        {
            ClockDivider = 4;
            OnePulse = new RmtCommand(14, true, 12, false);
            ZeroPulse = new RmtCommand(7, true, 16, false);
            ResetCommand = new RmtCommand(500, false, 520, false);
        }
    }
}
