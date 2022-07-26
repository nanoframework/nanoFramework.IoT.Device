// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Relay
{
    /// <summary>
    /// Unit 4 Relay. This module only contains 4 relays and 4 leds. Each can be piloted independly or being in sync.
    /// </summary>
    public class Unit4Relay : Base4Relay
    {
        /// <summary>
        /// Gets or sets a value indicating whether synchronize the Led with the Relay.
        /// </summary>
        public bool SynchronizedMode
        {
            get
            {
                return Read(Register.UnitSetup) == 1;
            }

            set
            {
                Write(Register.UnitSetup, (byte)(value ? 1 : 0));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Unit4Relay" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device.</param>
        public Unit4Relay(I2cDevice i2cDevice) : base(i2cDevice, RelayType.Unit)
        {
        }

        /// <summary>
        /// Sets all the Led in the same state. This preserved the relay state.
        /// </summary>
        /// <param name="state">The state On or Off.</param>
        public void SetAllLeds(State state)
        {
            byte relays = (byte)(Read(Register.UnitRelay) & 0x0F);
            relays |= (byte)(state == State.On ? 0xF0 : 0x00);
            Write(Register.UnitRelay, relays);
        }

        /// <summary>
        /// Sets all the Realys and the Leds to the same state.
        /// </summary>
        /// <param name="state">The state On or Off.</param>
        public void SetAllLedAndRelay(State state)
        {
            Write(Register.UnitRelay, (byte)(state == State.On ? 0xFF : 0x00));
        }

        /// <summary>
        /// Sets the state of a specific Led.
        /// </summary>
        /// <param name="number">The Led number from 0 to 3.</param>
        /// <param name="state">The state On or Off.</param>
        /// <exception cref="ArgumentException">Led number must be between 0 and 3.</exception>
        public void SetLed(byte number, State state)
        {
            if (number > 3)
            {
                throw new ArgumentException();
            }

            byte status = Read(Register.UnitRelay);
            if (state == State.On)
            {
                status &= (byte)~(0x10 << number);
            }
            else
            {
                status |= (byte)(0x10 << number);
            }

            Write(Register.UnitRelay, status);
        }        
    }
}
