// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MulticastDNS.Package
{
    internal interface IBitConverter
    {
        byte[] GetBytes(bool value);

        byte[] GetBytes(char value);

        byte[] GetBytes(short value);

        byte[] GetBytes(int value);

        byte[] GetBytes(long value);

        byte[] GetBytes(ushort value);

        byte[] GetBytes(uint value);

        byte[] GetBytes(ulong value);

        byte[] GetBytes(float value);

        byte[] GetBytes(double value);

        char ToChar(byte[] value, int index);

        short ToInt16(byte[] value, int index);

        int ToInt32(byte[] value, int index);

        long ToInt64(byte[] value, int index);

        ushort ToUInt16(byte[] value, int index);

        uint ToUInt32(byte[] value, int index);

        ulong ToUInt64(byte[] value, int index);

        float ToSingle(byte[] value, int index);

        double ToDouble(byte[] value, int index);

        bool ToBoolean(byte[] value, int index);
    }

    internal sealed class BigEndianBitConverter : EndianBitConverter
    {
        public override bool IsLittleEndian => false;

        protected override long FromBytesImpl(byte[] buffer, int index, int bytes)
        {
            long ret = 0;

            for (int i = 0; i < bytes; i++)
            {
                ret = unchecked(ret << 8 | buffer[index + i]);
            }

            return ret;
        }

        protected override byte[] ToBytesImpl(long value, int bytes)
        {
            byte[] buffer = new byte[bytes];
            int endOffset = bytes - 1;

            for (int i = 0; i < bytes; i++)
            {
                buffer[endOffset - i] = unchecked((byte)(value & 0xff));
                value >>= 8;
            }

            return buffer;
        }
    }

    internal sealed class LittleEndianBitConverter : EndianBitConverter
    {
        public override bool IsLittleEndian => true;

        protected override long FromBytesImpl(byte[] buffer, int index, int bytes)
        {
            long ret = 0;

            for (int i = 0; i < bytes; i++)
            {
                ret = unchecked(ret << 8 | buffer[index + bytes - 1 - i]);
            }

            return ret;
        }

        protected override byte[] ToBytesImpl(long value, int bytes)
        {
            byte[] buffer = new byte[bytes];

            for (int i = 0; i < bytes; i++)
            {
                buffer[i] = unchecked((byte)(value & 0xff));
                value >>= 8;
            }

            return buffer;
        }
    }

    internal abstract class EndianBitConverter : IBitConverter
    {
        public static IBitConverter Big => _big ??= new BigEndianBitConverter();

        public static IBitConverter Little => _little ??= new LittleEndianBitConverter();

        private static BigEndianBitConverter _big;

        private static LittleEndianBitConverter _little;

        public abstract bool IsLittleEndian { get; }

        protected EndianBitConverter()
        {
        }

        protected long FromBytes(byte[] buffer, int index, int bytes)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (index + bytes > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes));
            }

            return FromBytesImpl(buffer, index, bytes);
        }

        protected abstract long FromBytesImpl(byte[] buffer, int index, int bytes);

        protected byte[] ToBytes(long value, int bytes)
        {
            return ToBytesImpl(value, bytes);
        }

        protected abstract byte[] ToBytesImpl(long value, int bytes);

        public byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        public byte[] GetBytes(char value)
        {
            return ToBytes(value, 2);
        }

        public byte[] GetBytes(short value)
        {
            return ToBytes(value, 2);
        }

        public byte[] GetBytes(int value)
        {
            return ToBytes(value, 4);
        }

        public byte[] GetBytes(long value)
        {
            return ToBytes(value, 8);
        }

        public byte[] GetBytes(ushort value)
        {
            return ToBytes(value, 2);
        }

        public byte[] GetBytes(uint value)
        {
            return ToBytes(value, 4);
        }

        public byte[] GetBytes(ulong value)
        {
            return ToBytes(unchecked((long)value), 8);
        }

        public byte[] GetBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public byte[] GetBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        public char ToChar(byte[] value, int index = 0)
        {
            return unchecked((char)FromBytes(value, index, 2));
        }

        public short ToInt16(byte[] value, int index = 0)
        {
            return unchecked((short)FromBytes(value, index, 2));
        }

        public int ToInt32(byte[] value, int index = 0)
        {
            return unchecked((int)FromBytes(value, index, 4));
        }

        public long ToInt64(byte[] value, int index = 0)
        {
            return FromBytes(value, index, 8);
        }

        public ushort ToUInt16(byte[] value, int index = 0)
        {
            return unchecked((ushort)FromBytes(value, index, 2));
        }

        public uint ToUInt32(byte[] value, int index = 0)
        {
            return unchecked((uint)FromBytes(value, index, 4));
        }

        public ulong ToUInt64(byte[] value, int index = 0)
        {
            return unchecked((ulong)FromBytes(value, index, 8));
        }

        public float ToSingle(byte[] value, int index = 0)
        {
            return BitConverter.ToSingle(value, index);
        }

        public double ToDouble(byte[] value, int index = 0)
        {
            return BitConverter.ToDouble(value, index);
        }

        public bool ToBoolean(byte[] value, int index = 0)
        {
            return BitConverter.ToBoolean(value, index);
        }
    }
}
