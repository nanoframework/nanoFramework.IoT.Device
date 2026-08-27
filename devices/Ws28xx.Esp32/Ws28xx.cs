// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.Hardware.Esp32.Rmt;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Represents base class for WS28XX LED drivers (i.e. WS2812B or WS2808).
    /// </summary>
    public class Ws28xx : LedTransmitChannel
    {
        /// <summary>
        /// SPI device used for communication with the LED driver.
        /// </summary>
        protected readonly int GpioPin;

        /// <summary>
        /// Gets backing image to be updated on the driver.
        /// </summary>
        public BitmapImage Image { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ws28xx" /> class.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin used for communication with the LED driver.</param>
        /// <param name="image">The bitmap that represents the screen or led strip.</param>
        /// <param name="t0l">The T0L timing value.</param>
        /// <param name="t0h">The T0H timing value.</param>
        /// <param name="t1l">The T1L timing value.</param>
        /// <param name="t1h">The T1H timing value.</param>
        /// <param name="reset">The reset timing value.</param>
        public Ws28xx(int gpioPin, BitmapImage image, ushort t0l, ushort t0h, ushort t1l, ushort t1h, ushort reset)
            : base(gpioPin, t0l, t0h, t1l, t1h, reset)
        {
            if (gpioPin < 0)
            {
                throw new ArgumentException();
            }

            GpioPin = gpioPin;
            Image = image;
        }

        /// <summary>
        /// Sends backing image to the LED driver.
        /// </summary>
        public void Update()
        {
            SendLedData(Image.Data);
        }
    }
}
