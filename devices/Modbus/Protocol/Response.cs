using Iot.Device.Modbus.Util;
using System;

namespace Iot.Device.Modbus.Protocol
{
    class Response : Protocol
    {
        public Response(Request request)
        {
            this.DeviceId = request.DeviceId;
            this.Function = request.Function;
            this.Address = request.Address;
            this.Count = request.Count;

            if (this.Address < Consts.MinAddress || this.Address + this.Count > Consts.MaxAddress)
                this.ErrorCode = ErrorCode.IllegalDataAddress;
        }

        public Response(byte[] bytes)
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

        public byte DeviceId { get; private set; }
        public FunctionCode Function { get; private set; }
        public ushort Address { get; private set; }
        public ushort Count { get; private set; }

        public ErrorCode ErrorCode { get; set; } = ErrorCode.NoError;

        public byte[] Serialize()
        {
            var buffer = new DataBuffer(2);
            buffer.Set(0, DeviceId);

            var fn = (byte)Function;
            if (ErrorCode != ErrorCode.NoError)
            {
                fn = SetBit(fn, 8, true);	// 如果出错，将功能码最左边 Bit 设为 1
                buffer.Set(1, fn);
                buffer.Add((byte)ErrorCode);
            }
            else
            {
                buffer.Set(1, fn);
                switch (Function)
                {
                    case FunctionCode.ReadCoils:
                    case FunctionCode.ReadDiscreteInputs:
                    case FunctionCode.ReadHoldingRegisters:
                    case FunctionCode.ReadInputRegisters:
                        buffer.Add((byte)Data.Length);
                        buffer.Add(Data.Buffer);
                        break;
                    case FunctionCode.WriteMultipleCoils:
                    case FunctionCode.WriteMultipleRegisters:
                        buffer.Add(Address);
                        buffer.Add(Count);
                        break;
                    case FunctionCode.WriteSingleCoil:
                    case FunctionCode.WriteSingleRegister:
                        buffer.Add(Address);
                        buffer.Add(Data.Buffer);
                        break;
                    default:
                        buffer.Add(Data.Buffer);
                        break;
                }
            }

            var crc = Crc16.Calculate(buffer.Buffer);
            buffer.Add(crc);

            return buffer.Buffer;
        }

        private void Deserialize(byte[] bytes)
        {
            if (IsEmpty(bytes))
                return;

            var buffer = new DataBuffer(bytes);
            DeviceId = buffer.Get(0);

            byte[] crcBuff = buffer.Get(buffer.Length - 2, 2);
            byte[] crcCalc = Crc16.Calculate(bytes, 0, bytes.Length - 2);

            if ((crcBuff[0] != crcCalc[0] && crcBuff[0] != crcCalc[1]) ||
                (crcBuff[1] != crcCalc[1] && crcBuff[1] != crcCalc[0]))
            {
                IsValid = false;
                return;
            }

            byte fn = buffer.Get(1);
            if ((fn & Consts.ErrorMask) > 0)
            {
                Function = (FunctionCode)(fn ^ Consts.ErrorMask);
                ErrorCode = (ErrorCode)buffer.Get(2);
            }
            else
            {
                Function = (FunctionCode)fn;
                switch (Function)
                {
                    case FunctionCode.ReadCoils:
                    case FunctionCode.ReadDiscreteInputs:
                    case FunctionCode.ReadHoldingRegisters:
                    case FunctionCode.ReadInputRegisters:
                        byte length = buffer.Get(2);

                        // following bytes + 3 byte head + 2 byte CRC
                        if (buffer.Length < length + 5)
                            IsValid = false;
                        else
                        {
                            //if (buffer.Length > length + 5)
                            //	buffer = new DataBuffer((byte[])bytes.Slice(0, length + 5));

                            Data = new DataBuffer(buffer.Get(3, buffer.Length - 5));
                        }
                        break;
                    case FunctionCode.WriteMultipleCoils:
                    case FunctionCode.WriteMultipleRegisters:
                        Address = buffer.GetUInt16(2);
                        Count = buffer.GetUInt16(4);
                        break;
                    case FunctionCode.WriteSingleCoil:
                    case FunctionCode.WriteSingleRegister:
                        Address = buffer.GetUInt16(2);
                        Data = new DataBuffer(buffer.Get(4, buffer.Length - 6));
                        break;
                    default:
                        Data = new DataBuffer(buffer.Get(2, buffer.Length - 4));
                        break;
                }
            }
        }

        /// <param name="data"></param>
        /// <param name="index">From left to right: 8,7,6,5,4,3,2,1</param>
        /// <param name="flag">true: 1 / false: 0</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        static byte SetBit(byte data, byte index, bool flag)
        {
            if (index > 8 || index < 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            int v = index < 2 ? index : (2 << (index - 2));
            return flag ? (byte)(data | v) : (byte)(data & ~v);
        }
    }
}