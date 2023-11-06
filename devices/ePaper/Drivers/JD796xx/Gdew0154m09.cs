// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.EPaper.Drivers.JD796xx
{
    /// <summary>
    /// A driver class for the GDEW0154M09 display controller.
    /// </summary>
    public class Gdew0154m09 : JD79653A
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gdew0154m09"/> class.
        /// </summary>
        /// <param name="spiDevice">The communication channel to the GDEW0154M09-based dispay.</param>
        /// <param name="resetPin">The reset GPIO pin. Passing an invalid pin number such as -1 will prevent this driver from opening the pin. Caller should handle hardware resets.</param>
        /// <param name="busyPin">The busy GPIO pin.</param>
        /// <param name="dataCommandPin">The data/command GPIO pin.</param>
        /// <param name="gpioController">The <see cref="GpioController"/> to use when initializing the pins.</param>
        /// <param name="enableFramePaging">Page the frame buffer and all operations to use less memory.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        /// <exception cref="ArgumentNullException"><paramref name="spiDevice"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Display width and height can't be less than 0 or greater than 200.</exception>
        /// <remarks>
        /// For a 200x200 GDEW0154M09 display, a full Frame requires about 5KB of RAM ((200 * 200) / 8).
        /// If you can't guarantee 5KB to be available to the driver then enable paging by setting <paramref name="enableFramePaging"/> to true.
        /// A page uses about 1KB (5KB / PagesPerFrame).
        /// </remarks>
        public Gdew0154m09(
            SpiDevice spiDevice,
            int resetPin,
            int busyPin,
            int dataCommandPin,
            GpioController gpioController = null,
            bool enableFramePaging = false,
            bool shouldDispose = true) : base(spiDevice, resetPin, busyPin, dataCommandPin, 200, 200, gpioController, enableFramePaging, shouldDispose)
        {
        }

        /// <inheritdoc/>
        protected override int PagesPerFrame { get; } = 5;
    }
}
