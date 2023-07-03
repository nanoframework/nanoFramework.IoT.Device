// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Modbus.Structures
{
    abstract class ModbusObject
    {
        public ushort Address { get; set; }
        public byte HiByte { get; set; }
        public byte LoByte { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is not ModbusObject mo)
            {
                return false;
            }

            return GetType() == mo.GetType()
                && Address == mo.Address
                && HiByte == mo.HiByte
                && LoByte == mo.LoByte;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => base.GetHashCode() ^
                Address.GetHashCode() ^
                HiByte.GetHashCode() ^
                LoByte.GetHashCode();
    }
}
