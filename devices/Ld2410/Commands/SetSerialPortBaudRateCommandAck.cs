// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ld2410.Commands
{
    internal sealed class SetSerialPortBaudRateCommandAck : CommandAckFrame
    {
        internal SetSerialPortBaudRateCommandAck(bool isSuccess)
            : base(CommandWord.SetBaudRate, isSuccess)
        {
        }
    }
}
