// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ld2410.Commands
{
    internal sealed class RestartCommandAck : CommandAckFrame
    {
        internal RestartCommandAck(bool isSuccess)
            : base(CommandWord.Restart, isSuccess)
        {
        }
    }
}
