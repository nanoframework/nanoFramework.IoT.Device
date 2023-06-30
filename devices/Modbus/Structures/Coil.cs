namespace Iot.Device.Modbus.Structures
{
    class Coil : ModbusObject
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
