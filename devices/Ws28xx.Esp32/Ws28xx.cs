// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using nanoFramework.Hardware.Esp32.Rmt;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Represents base class for WS28XX LED drivers (i.e. WS2812B or WS2808).
    /// </summary>
    public class Ws28xx
    {
        /// <summary>
        /// SPI device used for communication with the LED driver.
        /// </summary>
        protected readonly int GpioPin;

        /// <summary>
        /// Gets or sets the One pulse command.
        /// </summary>
        public RmtCommand OnePulse { get; set; }

        /// <summary>
        /// Gets or sets the zero pulse command.
        /// </summary>
        public RmtCommand ZeroPulse { get; set; }

        /// <summary>
        /// Gets or sets the reset command.
        /// </summary>
        public RmtCommand ResetCommand { get; set; }

        /// <summary>
        /// Gets or sets the clock divider.
        /// </summary>
        public byte ClockDivider { get; set; } = 2;

        /// <summary>
        /// Gets backing image to be updated on the driver.
        /// </summary>
        public BitmapImage Image { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ws28xx" /> class.
        /// </summary>
        /// <param name="gpioPin">The GPIO pin used for communication with the LED driver.</param>
        /// <param name="image">The bitmap that represents the screen or led strip.</param>
        public Ws28xx(int gpioPin, BitmapImage image)
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
            var transmitterSettings = new TransmitChannelSettings(pinNumber: GpioPin)
            {
                EnableCarrierWave = false,
                IdleLevel = false,

                // this value for the clock divider considers a clock source of 80MHz which is what we have fixed in native
                ClockDivider = this.ClockDivider,
            };
            var transmitter = new TransmitterChannel(transmitterSettings);

            for (int i = 0; i < Image.Data.Length; i++)
            {
                SerializeColor(Image.Data[i], transmitter);
            }

            transmitter.AddCommand(ResetCommand);
            transmitter.Send(true);
            transmitter.Dispose();
        }

        private void SerializeColor(byte b, TransmitterChannel transmitter)
        {
            for (var i = 0; i < 8; ++i)
            {
                transmitter.AddCommand(((b & (1u << 7)) != 0) ? OnePulse : ZeroPulse);
                b <<= 1;
            }
        }
    }
}
