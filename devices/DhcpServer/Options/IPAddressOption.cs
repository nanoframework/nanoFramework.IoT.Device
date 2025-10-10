// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;
using System;
using System.Net;

namespace Iot.Device.DhcpServer.Options
{
    /// <summary>
    /// Represents a DHCP option with a single <see cref="IPAddress"/> value.
    /// </summary>
    public class IPAddressOption : Option
    {
        private IPAddress? _value;

        /// <summary>
        /// Creates a new <see cref="IPAddressOption"/> with the specified <paramref name="code"/> and <paramref name="data"/>.
        /// </summary>
        public IPAddressOption(
            DhcpOptionCode code,
            byte[] data) : base(code, data)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Creates a new <see cref="IPAddressOption"/> with the specified <paramref name="code"/> and <paramref name="value"/>.
        /// </summary>
        public IPAddressOption(
            DhcpOptionCode code,
            IPAddress value) : this(code, Converter.GetBytes(value))
        {
            _value = value;
        }

        /// <summary>
        /// Gets the <see cref="IPAddress"/> set for this DHCP option.
        /// </summary>
        public IPAddress Deserialize()
        {
            return _value ??= Converter.GetIPAddress(Data);
        }

        internal static bool IsKnownOption(byte code) => IsKnownOption((DhcpOptionCode)code);

        internal static bool IsKnownOption(DhcpOptionCode code)
        {
            return code switch
            {
                DhcpOptionCode.RequestedIpAddress=> true,
                DhcpOptionCode.ServerIdentifier => true,
                DhcpOptionCode.SubnetMask => true,
                _ => false
            };
        }

        /// <inheritdoc />
        public override string ToString() => ToString(Deserialize());
    }
}
