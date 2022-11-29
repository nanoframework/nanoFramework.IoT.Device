// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;

using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Enums;

namespace Iot.Device.EPaper.Drivers.Ssd168x.Ssd1680
{
    /// <summary>
    /// A driver class for the SSD1680 display controller.
    /// </summary>
    public sealed class Ssd1680 : Ssd168x
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ssd1680"/> class.
        /// </summary>
        /// <param name="spiDevice">The communication channel to the SSD1680-based dispay.</param>
        /// <param name="resetPin">The reset GPIO pin. Passing an invalid pin number such as -1 will prevent this driver from opening the pin. Caller should handle hardware resets.</param>
        /// <param name="busyPin">The busy GPIO pin.</param>
        /// <param name="dataCommandPin">The data/command GPIO pin.</param>
        /// <param name="width">The width of the display.</param>
        /// <param name="height">The height of the display.</param>
        /// <param name="gpioController">The <see cref="GpioController"/> to use when initializing the pins.</param>
        /// <param name="enableFramePaging">Page the frame buffer and all operations to use less memory.</param>
        /// <param name="shouldDispose"><see langword="true"/> to dispose of the <see cref="GpioController"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="spiDevice"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Display width and height can't be less than 0 or greater than 176 and 296 respectively.</exception>
        /// <remarks>
        /// For a 176x296 SSD1680 display, a full Frame requires about 6KB of RAM ((176 * 296) / 8). SSD1680 has 2 RAMs for B/W and Red pixel.
        /// This means to have a full frame in memory, you need about 12KB of RAM. If you can't guarantee 12KB to be available to the driver
        /// then enable paging by setting <paramref name="enableFramePaging"/> to true. A page uses about 3KB (1.5KB for B/W and 1.5KB for Red).
        /// </remarks>
        public Ssd1680(
            SpiDevice spiDevice,
            int resetPin,
            int busyPin,
            int dataCommandPin,
            int width,
            int height,
            GpioController gpioController = null,
            bool enableFramePaging = true,
            bool shouldDispose = true) : base(spiDevice, resetPin, busyPin, dataCommandPin, width, height, gpioController, enableFramePaging, shouldDispose)
        {
            if (width <= 0 || width > 176)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0 || height > 296)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
        }

        /// <inheritdoc/>
        protected override int PagesPerFrame { get; } = 4;
    }
}
