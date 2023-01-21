// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// Represents base class for WS28XX LED drivers (i.e. WS2812B or WS2808).
    /// </summary>
    public class Ws28xx
    {
        /// <summary>
        /// SPI device used for communication with the LED driver.
        /// </summary>
        protected readonly SpiDevice SpiDevice;

        /// <summary>
        /// Gets backing image to be updated on the driver.
        /// </summary>
        public BitmapImage Image { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ws28xx" /> class.
        /// </summary>
        /// <param name="spiDevice">SPI device used for communication with the LED driver.</param>
        /// <param name="image">The bitmap that represents the screen or led strip.</param>
        public Ws28xx(SpiDevice spiDevice, BitmapImage image)
        {
            SpiDevice = spiDevice ?? throw new ArgumentNullException(nameof(spiDevice));
            Image = image;
        }

        /// <summary>
        /// Sends backing image to the LED driver.
        /// </summary>
        public void Update() => SpiDevice.Write(Image.Data);
    }
}
