using System;

namespace Iot.Device.Modbus.Structures
{
    abstract class Register : ModbusObject
    {
        public ushort Value
        {
            get
            {
                var blob = new[] { HiByte, LoByte };
                if (BitConverter.IsLittleEndian)
                    blob.JudgReverse();

                return BitConverter.ToUInt16(blob, 0);
            }
            set
            {
                var blob = BitConverter.GetBytes(value);
                if (BitConverter.IsLittleEndian)
                    blob.JudgReverse();

                HiByte = blob[0];
                LoByte = blob[1];
            }
        }
    }

    class HoldingRegister : Register
    {
    }

    class InputRegister : Register
    {
    }
}
