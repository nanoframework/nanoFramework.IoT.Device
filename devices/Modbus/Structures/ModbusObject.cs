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
                return false;

            return this.GetType() == mo.GetType()
                && this.Address == mo.Address
                && this.HiByte == mo.HiByte
                && this.LoByte == mo.LoByte;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
            => base.GetHashCode() ^
                Address.GetHashCode() ^
                HiByte.GetHashCode() ^
                LoByte.GetHashCode();
    }
}
