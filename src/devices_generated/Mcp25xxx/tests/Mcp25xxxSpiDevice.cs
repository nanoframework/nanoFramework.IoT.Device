// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;

namespace Iot.Device.Mcp25xxx.Tests
{
    public class Mcp25xxxSpiDevice : SpiDevice
    {
        public override SpiConnectionSettings ConnectionSettings => throw new NotImplementedException();
        public byte[]? LastReadBuffer { get; set; }

        public byte LastReadByte { get; set; }

        public byte[]? LastWriteBuffer { get; private set; }

        public byte LastWriteByte { get; private set; }

        public override void Read(SpanByte buffer)
        {
            LastReadBuffer = buffer.ToArray();
        }

        public override byte ReadByte() => LastReadByte;

        public override void TransferFullDuplex(SpanByte writeBuffer, SpanByte readBuffer)
        {
            LastWriteBuffer = writeBuffer.ToArray();
            LastReadBuffer = readBuffer.ToArray();
        }

        public override void Write(SpanByte buffer)
        {
            LastWriteBuffer = buffer.ToArray();
        }

        public override void WriteByte(byte value)
        {
            LastWriteByte = value;
        }
    }
}
