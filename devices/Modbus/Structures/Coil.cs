// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Modbus.Structures
{
    internal class Coil : ModbusObject
    {
        public bool Value
        {
            get
            {
                return HiByte > 0 || LoByte > 0;
            }

            set
            {
                HiByte = 0;
                LoByte = (byte)(value ? 1 : 0);
            }
        }
    }
}
