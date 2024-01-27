// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ld2410.Commands
{
    internal sealed class EnableConfigurationCommandAck : CommandAckFrame
    {
        internal ushort ProtocolVersion { get; }

        internal ushort BufferSize { get; }

        internal EnableConfigurationCommandAck(
            bool isSuccess,
            ushort protocolVersion,
            ushort bufferSize) : base(CommandWord.EnableConfiguration, isSuccess)
        {
            this.ProtocolVersion = protocolVersion;
            this.BufferSize = bufferSize;
        }
    }
}
