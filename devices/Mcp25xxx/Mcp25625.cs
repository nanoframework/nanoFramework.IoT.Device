// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Spi;

namespace Iot.Device.Mcp25xxx
{
    /// <summary>
    /// Driver for the Microchip MCP25625 CAN controller.
    /// </summary>
    public class Mcp25625 : Mcp25xxx
    {
        private readonly int _standby;

        /// <summary>
        /// Initializes a new instance of the Mcp25625 class.
        /// </summary>
        /// <param name="spiDevice">The SPI device used for communication.</param>
        /// <param name="reset">The output pin number that is connected to Reset.</param>
        /// <param name="tx0rts">The output pin number that is connected to Tx0RTS.</param>
        /// <param name="tx1rts">The output pin number that is connected to Tx1RTS.</param>
        /// <param name="tx2rts">The output pin number that is connected to Tx2RTS.</param>
        /// <param name="standby">The output pin number that is connected to STBY.</param>
        /// <param name="interrupt">The input pin number that is connected to INT.</param>
        /// <param name="rx0bf">The input pin number that is connected to Rx0BF.</param>
        /// <param name="rx1bf">The input pin number that is connected to Rx1BF.</param>
        /// <param name="clkout">The input pin number that is connected to CLKOUT.</param>
        /// <param name="gpioController">
        /// The GPIO controller for defined external pins. If not specified, the default controller will be used.
        /// </param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        public Mcp25625(
            SpiDevice spiDevice,
            int reset = -1,
            int tx0rts = -1,
            int tx1rts = -1,
            int tx2rts = -1,
            int standby = -1,
            int interrupt = -1,
            int rx0bf = -1,
            int rx1bf = -1,
            int clkout = -1,
            GpioController? gpioController = null,
            bool shouldDispose = true)
            : base(
                  spiDevice,
                  reset,
                  tx0rts,
                  tx1rts,
                  tx2rts,
                  interrupt,
                  rx0bf,
                  rx1bf,
                  clkout,
                  gpioController,
                  shouldDispose)
        {
            _standby = standby;

            if (_standby != -1)
            {
                // Controller should already be configured if other pins are used.
                _gpioController = _gpioController ?? new GpioController();
                _gpioController.OpenPin(_standby, PinMode.Output);
            }
        }

        /// <summary>
        /// Writes a value to Standby (STBY) pin.
        /// </summary>
        public PinValue StandbyPin
        {
            set
            {
                if (_gpioController is object)
                {
                    _gpioController.Write(_standby, value);
                    return;
                }

                throw new Exception("GPIO controller is not correctly configured");
            }
        }
    }
}
