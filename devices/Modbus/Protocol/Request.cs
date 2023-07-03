// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Modbus.Util;

namespace Iot.Device.Modbus.Protocol
{
    internal class Request : Protocol
    {
        public Request()
        {
        }

        public Request(byte[] bytes)
        {
            try
            {
                this.Deserialize(bytes);
            }
            catch
            {
                IsValid = false;
            }
        }

        public byte DeviceId { get; set; }

        public FunctionCode Function { get; set; }

        public ushort Address { get; set; }

        public ushort Count { get; set; }

        public byte[] Serialize()
        {
            var buffer = new DataBuffer(2);
            buffer.Set(0, DeviceId);
            buffer.Set(1, (byte)Function);

            switch (Function)
            {
                case FunctionCode.ReadCoils:
                case FunctionCode.ReadDiscreteInputs:
                case FunctionCode.ReadHoldingRegisters:
                case FunctionCode.ReadInputRegisters:
                    buffer.Add(Address);
                    buffer.Add(Count);
                    break;
                case FunctionCode.WriteMultipleCoils:
                case FunctionCode.WriteMultipleRegisters:
                    buffer.Add(Address);
                    buffer.Add(Count);
                    if (Data != null && Data.Length > 0)
                    {
                        buffer.Add(Data.Buffer);
                    }

                    break;
                case FunctionCode.WriteSingleCoil:
                case FunctionCode.WriteSingleRegister:
                    buffer.Add(Address);
                    if (Data != null && Data.Length > 0)
                    {
                        buffer.Add(Data.Buffer);
                    }

                    break;
                default:
                    if (Data != null && Data.Length > 0)
                    {
                        buffer.Add(Data.Buffer);
                    }

                    break;
            }

            byte[] crc = Crc16.Calculate(buffer.Buffer);
            buffer.Add(crc);

            return buffer.Buffer;
        }

        private void Deserialize(byte[] bytes)
        {
            if (IsEmpty(bytes))
            {
                return;
            }

            var buffer = new DataBuffer(bytes);
            DeviceId = buffer.Get(0);

            byte[] crcBuff = buffer.Get(buffer.Length - 2, 2);
            byte[] crcCalc = Crc16.Calculate(bytes, 0, bytes.Length - 2);

            if (crcBuff[0] != crcCalc[0] || crcBuff[1] != crcCalc[1])
            {
                IsValid = false;
                return;
            }

            Function = (FunctionCode)buffer.Get(1);
            switch (Function)
            {
                case FunctionCode.ReadCoils:
                case FunctionCode.ReadDiscreteInputs:
                case FunctionCode.ReadHoldingRegisters:
                case FunctionCode.ReadInputRegisters:
                    Address = buffer.GetUInt16(2);
                    Count = buffer.GetUInt16(4);
                    break;
                case FunctionCode.WriteMultipleCoils:
                case FunctionCode.WriteMultipleRegisters:
                    Address = buffer.GetUInt16(2);
                    Count = buffer.GetUInt16(4);
                    Data = new DataBuffer(buffer.Get(7, buffer.Length - 9));
                    break;
                case FunctionCode.WriteSingleCoil:
                case FunctionCode.WriteSingleRegister:
                    Address = buffer.GetUInt16(2);
                    Count = 1;
                    Data = new DataBuffer(buffer.Get(4, buffer.Length - 6));
                    break;
                default:
                    Data = new DataBuffer(buffer.Get(2, buffer.Length - 4));
                    break;
            }
        }
    }
}
