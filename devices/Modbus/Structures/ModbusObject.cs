// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Modbus.Structures
{
    internal abstract class ModbusObject
    {
        public ushort Address { get; set; }
        
        public byte HiByte { get; set; }
        
        public byte LoByte { get; set; }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            // We cannot do obj is not ModbusObject mo as StyleCop do not support this syntax
            if (obj.GetType() != typeof(ModbusObject))
            {
                return false;
            }

            ModbusObject mo = (ModbusObject)obj;
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
