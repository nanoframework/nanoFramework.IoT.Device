// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.DhcpServer
{
    internal static class MessageIndex
    {
        public const ushort Operation = 0; // 1 octet
        public const ushort HardwareAddressType = 1; // 1 octet
        public const ushort HardwareAddressLength = 2; // 1 octet
        public const ushort Hops = 3; // 1 octet
        public const ushort TransactionId = 4; // 4 octets
        public const ushort SecondsElapsed = 8; // 2 octets
        public const ushort Flags = 10; // 2 octets
        public const ushort ClientIPAddress = 12; // 4 octets
        public const ushort YourIPAddress = 16; // 4 octets
        public const ushort ServerIPAddress = 20; // 4 octets
        public const ushort GatewayIPAddress = 24; // 4 octets
        public const ushort HardwareAddress = 28; // 16 octets
        public const ushort ServerHostName = 44; // 64 octets
        public const ushort File = 108; // 128 octets
        public const ushort MagicCookie = 236; // 4 octets
        public const ushort Options = 240; // variable 
    }
}
