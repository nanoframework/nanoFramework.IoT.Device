// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;

namespace Iot.Device.DhcpServer.Options
{
    internal class UnknownOption : Option
    {
        public UnknownOption(DhcpOptionCode code, byte[] data) : base(code, data)
        {
        }

        public override string ToString() => ToString("??");
    }
}
