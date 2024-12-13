// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System;

namespace Iot.Device.MulticastDns.Package
{
    internal sealed class LittleEndianBitConverter : EndianBitConverter
    {
        public override bool IsLittleEndian => false;

        public override byte[] GetBytes(char value)
        {
            byte[] result = new byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(result, (short)value);
            return result;
        }

        public override byte[] GetBytes(short value)
        {
            byte[] result = new byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(result, value);
            return result;
        }

        public override byte[] GetBytes(int value)
        {
            byte[] result = new byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(result, value);
            return result;
        }

        public override byte[] GetBytes(long value)
        {
            byte[] result = new byte[8];
            BinaryPrimitives.WriteInt64LittleEndian(result, value);
            return result;
        }

        public override byte[] GetBytes(ushort value)
        {
            byte[] result = new byte[2];
            BinaryPrimitives.WriteUInt16LittleEndian(result, value);
            return result;
        }

        public override byte[] GetBytes(uint value)
        {
            byte[] result = new byte[4];
            BinaryPrimitives.WriteUInt32LittleEndian(result, value);
            return result;
        }

        public override byte[] GetBytes(ulong value)
        {
            byte[] result = new byte[8];
            BinaryPrimitives.WriteUInt64LittleEndian(result, value);
            return result;
        }

        public override byte[] GetBytes(float value)
        {
            byte[] result = new byte[4];
            BinaryPrimitives.WriteSingleLittleEndian(result, value);
            return result;
        }

        public override byte[] GetBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        public override char ToChar(SpanByte value, int index = 0)
        {
            return unchecked((char)BinaryPrimitives.ReadInt16LittleEndian(value.Slice(index, 2)));
        }

        public override short ToInt16(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt16LittleEndian(value.Slice(index, 2));
        }

        public override int ToInt32(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt32LittleEndian(value.Slice(index, 4));
        }

        public override long ToInt64(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadInt64LittleEndian(value.Slice(index, 8));
        }

        public override ushort ToUInt16(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt16LittleEndian(value.Slice(index, 2));
        }

        public override uint ToUInt32(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt32LittleEndian(value.Slice(index, 4));
        }

        public override ulong ToUInt64(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadUInt64LittleEndian(value.Slice(index, 8));
        }

        public override float ToSingle(SpanByte value, int index = 0)
        {
            return BinaryPrimitives.ReadSingleLittleEndian(value.Slice(index, 4));
        }

        public override double ToDouble(SpanByte value, int index = 0)
        {
            byte[] bytes = value.ToArray();
            return BitConverter.ToDouble(bytes, index);
        }
    }
}
