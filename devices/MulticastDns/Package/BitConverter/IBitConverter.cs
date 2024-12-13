// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.MulticastDns.Package
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

        char ToChar(SpanByte value, int index);

        short ToInt16(SpanByte value, int index);

        int ToInt32(SpanByte value, int index);

        long ToInt64(SpanByte value, int index);

        ushort ToUInt16(SpanByte value, int index);

        uint ToUInt32(SpanByte value, int index);

        ulong ToUInt64(SpanByte value, int index);

        float ToSingle(SpanByte value, int index);

        double ToDouble(SpanByte value, int index);

        bool ToBoolean(SpanByte value, int index);
    }
}
