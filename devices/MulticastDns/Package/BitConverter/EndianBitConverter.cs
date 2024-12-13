// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MulticastDns.Package
{
    internal abstract class EndianBitConverter : IBitConverter
    {
        public static IBitConverter Big => _big ??= new BigEndianBitConverter();

        public static IBitConverter Little => _little ??= new LittleEndianBitConverter();

        private static BigEndianBitConverter _big;

        private static LittleEndianBitConverter _little;

        public abstract bool IsLittleEndian { get; }

        public byte[] GetBytes(bool value) => new byte[] { value ? (byte)0x1 : (byte)0x0 };

        public abstract byte[] GetBytes(char value);

        public abstract byte[] GetBytes(short value);

        public abstract byte[] GetBytes(int value);

        public abstract byte[] GetBytes(long value);

        public abstract byte[] GetBytes(ushort value);

        public abstract byte[] GetBytes(uint value);

        public abstract byte[] GetBytes(ulong value);

        public abstract byte[] GetBytes(float value);

        public abstract byte[] GetBytes(double value);

        public bool ToBoolean(SpanByte value, int index = 0) => value[index] == 0x1;

        public abstract char ToChar(SpanByte value, int index = 0);

        public abstract short ToInt16(SpanByte value, int index = 0);

        public abstract int ToInt32(SpanByte value, int index = 0);

        public abstract long ToInt64(SpanByte value, int index = 0);

        public abstract ushort ToUInt16(SpanByte value, int index = 0);

        public abstract uint ToUInt32(SpanByte value, int index = 0);

        public abstract ulong ToUInt64(SpanByte value, int index = 0);

        public abstract float ToSingle(SpanByte value, int index = 0);

        public abstract double ToDouble(SpanByte value, int index = 0);
    }
}
