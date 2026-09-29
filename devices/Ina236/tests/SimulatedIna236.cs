// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Adc.Tests
{
    internal class SimulatedIna236 : IIna236I2cDevice
    {
        private readonly ushort[] _registers = new ushort[64];
        private byte _currentRegister;

        public SimulatedIna236()
        {
            _registers[0] = 0x4127;
            _registers[1] = 19200;
            _registers[2] = 7500;
            _registers[3] = 4500;
            _registers[4] = 12000;
            _registers[0x3F] = 0xA080;
        }

        public void Read(SpanByte buffer)
        {
            ushort value = _registers[_currentRegister];
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)value;
        }

        public void Write(SpanByte buffer)
        {
            _currentRegister = buffer[0];
            if (buffer.Length == 3)
            {
                ushort value = (ushort)((buffer[1] << 8) | buffer[2]);
                _registers[_currentRegister] = _currentRegister == 0 && (value & 0x8000) != 0 ? (ushort)0x4127 : value;
            }
        }

        public ushort GetRegister(byte register) => _registers[register];

        public void SetRegister(byte register, ushort value) => _registers[register] = value;

        public void Dispose()
        {
        }
    }
}