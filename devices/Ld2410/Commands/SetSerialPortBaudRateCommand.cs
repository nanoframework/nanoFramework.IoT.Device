// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;

namespace Iot.Device.Ld2410.Commands
{
    internal sealed class SetSerialPortBaudRateCommand : CommandFrame
    {
        public SetSerialPortBaudRateCommand(BaudRate baudRate = BaudRate.BaudRate256000)
            : base(CommandWord.SetBaudRate)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(Value = new byte[2], (ushort)baudRate);
        }
    }
}
