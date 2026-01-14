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
    public class Ssd1306 : Ssd13xx
    {
        /// <summary>
        /// Default I2C bus address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x3C;

        /// <summary>
        /// Secondary I2C bus address.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x3D;

        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ssd1306" /> class.
        /// A single-chip CMOS OLED/PLED driver with controller for organic/polymer
        /// light emitting diode dot-matrix graphic display system.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="resetPin">Reset pin (some displays might be wired to share the microcontroller's
        /// reset pin).</param>
        public Ssd1306(I2cDevice i2cDevice, int resetPin = -1) : base(i2cDevice, DisplayResolution.OLED128x64, DisplayOrientation.Landscape, resetPin)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ssd1306" /> class.
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
        public Ssd1306(I2cDevice i2cDevice, DisplayResolution res, DisplayOrientation orientation = DisplayOrientation.Landscape, int resetPin = -1, GpioController gpio = null, bool shouldDispose = true) : base(i2cDevice, res, orientation, resetPin, gpio, shouldDispose)
        {
        }

        /// <summary>
        /// Sends command to the device.
        /// </summary>
        /// <param name="command">Command being send.</param>
        public void SendCommand(ISsd1306Command command) => SendCommand((ICommand)command);

        /// <summary>
        /// Sends command to the device.
        /// </summary>
        /// <param name="command">Command being send.</param>
        public override void SendCommand(ISharedCommand command) => SendCommand(command);

        /// <summary>
        /// Send a command to the display controller.
        /// </summary>
        /// <param name="command">The command to send to the display controller.</param>
        private void SendCommand(ICommand command)
        {
            Span<byte> commandBytes = command?.GetBytes();

            if (commandBytes.Length == 0)
            {
                throw new ArgumentNullException(nameof(command), "Argument is either null or there were no bytes to send.");
            }

            Span<byte> writeBuffer = SliceGenericBuffer(commandBytes.Length + 1);
            writeBuffer[0] = 0x00; // Control byte
            commandBytes.CopyTo(writeBuffer.Slice(1));

            // Be aware there is a Continuation Bit in the Control byte and can be used
            // to state (logic LOW) if there is only data bytes to follow.
            // This binding separates commands and data by using SendCommand and SendData.
            _i2cDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Internal cleanup.
        /// </summary>
        /// <param name="disposing">Should dispose managed resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                base.Dispose(disposing);
                _disposed = true;
            }
        }
    }
}
