// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.DhcpServer.Enums;

namespace Iot.Device.DhcpServer.Options
{
    internal class MessageTypeOption : Option
    {
        public static bool IsKnownOption(byte code) => IsKnownOption((DhcpOptionCode)code);

        public static bool IsKnownOption(DhcpOptionCode code) => DhcpOptionCode.DhcpMessageType == code;

        public MessageTypeOption(byte[] data) : base(DhcpOptionCode.DhcpMessageType, data)
        {
            if (data.Length != 1)
            {
                throw new ArgumentException();
            }
        }

        public MessageTypeOption(DhcpMessageType messageType) : this(new[] { (byte)messageType })
        {
        }

        public DhcpMessageType Deserialize()
        {
            return (DhcpMessageType)Data[0];
        }

        public override string ToString() => ToString(Deserialize());
    }
}
