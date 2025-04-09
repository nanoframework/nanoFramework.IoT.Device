// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using Iot.Device.Ssd13xx.Commands;

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
    /// light emitting diode dot-matrix graphic display system.
    /// </summary>
    public class Sh1106 : Ssd1306
    {
        private byte[] _pageCmd = new byte[]
        {
            0x00, 
            0xB0, 
            0x02, 
            0x10
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Sh1106" /> class.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="resetPin">Reset pin (some displays might be wired to share the microcontroller's
        /// reset pin).</param>
        public Sh1106(I2cDevice i2cDevice, int resetPin = -1) : base(i2cDevice, DisplayResolution.OLED128x64, DisplayOrientation.Landscape, resetPin)
        {
            SetPageCmd(_pageCmd);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sh1106" /> class.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="res">Display resolution.</param>
        /// <param name="orientation">Rotation of the display with reference to the hardware's default orientation.</param>
        /// <param name="resetPin">Reset pin (some displays might be wired to share the microcontroller's
        /// reset pin).</param>
        /// <param name="gpio">Gpio Controller.</param> 
        /// <param name="shouldDispose">True to dispose the GpioController.</param>        
        public Sh1106(I2cDevice i2cDevice, DisplayResolution res, DisplayOrientation orientation = DisplayOrientation.Landscape, int resetPin = -1, GpioController gpio = null, bool shouldDispose = true) : base(i2cDevice, res, orientation, resetPin, gpio, shouldDispose)
        {
            SetPageCmd(_pageCmd);
        }
    }
}
