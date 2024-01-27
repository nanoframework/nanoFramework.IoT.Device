// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Modbus.Structures
{
    internal abstract class Register : ModbusObject
    {
        public short Value
        {
            get
            {
                var blob = new[] { HiByte, LoByte };

                if (BitConverter.IsLittleEndian)
                {
                    blob.JudgReverse();
                }

                return BitConverter.ToInt16(blob, 0);
            }

            set
            {
                var blob = BitConverter.GetBytes(value);
                if (BitConverter.IsLittleEndian)
                {
                    blob.JudgReverse();
                }

                HiByte = blob[0];
                LoByte = blob[1];
            }
        }
    }
}
