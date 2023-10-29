// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ld2410.Commands
{
    internal abstract class CommandAckFrame
    {
        internal CommandWord Command { get; }

        internal bool IsSuccess { get; }

        protected CommandAckFrame(CommandWord command, bool isSuccess)
        {
            Command = command;
            IsSuccess = isSuccess;
        }
    }
}
