// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ld2410.Commands
{
    internal sealed class ReadFirmwareVersionCommandAck : CommandAckFrame
    {
        internal ushort FirmwareType { get; }

        internal byte Major { get; }

        internal byte Minor { get; }

        internal byte[] Patch { get; }

        internal ReadFirmwareVersionCommandAck(bool isSuccess, ushort firmwareType, byte major, byte minor, byte[] patch)
            : base(CommandWord.ReadFirmwareVersion, isSuccess)
        {
            this.FirmwareType = firmwareType;
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }
    }
}
