// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Relay
{
    /// <summary>
    /// Base relay common to both Module and Unit 4 Relay.
    /// </summary>
    public class Base4Relay
    {
        /// <summary>
        /// 4 Relay default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x26;

        private I2cDevice _i2cDevice;
        private RelayType _relayType;

        internal Base4Relay(I2cDevice i2cDevice, RelayType relayType)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _relayType = relayType;
        }

        /// <summary>
        /// Sets the state of a specific Relay.
        /// </summary>
        /// <param name="number">The Relay number from 0 to 3.</param>
        /// <param name="state">The state On or Off.</param>
        /// <exception cref="ArgumentException">Relay number must be between 0 and 3.</exception>
        public void SetRelay(byte number, State state)
        {
            if (number > 3)
            {
                throw new ArgumentException();
            }

            Register reg = _relayType == RelayType.Unit ? Register.UnitRelay : Register.ModuleRelay;
            byte status = Read(reg);
            if (state == State.On)
            {
                status &= (byte)~(0x01 << number);
            }
            else
            {
                status |= (byte)(0x01 << number);
            }

            Write(Register.UnitRelay, status);
        }

        /// <summary>
        /// Sets all relays in the desired state.
        /// </summary>
        /// <param name="state">The state On or Off.</param>
        public void SetAllRelays(State state)
        {
            Register reg = _relayType == RelayType.Unit ? Register.UnitRelay : Register.ModuleRelay;
            byte relays = (byte)(Read(reg) & 0xF0);
            relays |= (byte)(state == State.On ? 0x0F : 0x00);
            Write(reg, relays);
        }

        internal byte Read(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        internal void Write(Register reg, byte val)
        {
            SpanByte span = new byte[2] { (byte)reg, val };
            _i2cDevice.Write(span);
        }
    }
}
