// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;

namespace Iot.Device.DhcpServer.Options
{
    /// <summary>
    /// Represents a DHCP option with a <see cref="string"/> value.
    /// </summary>
    /// <remarks>This option does not support strings that need to be null terminated.</remarks>
    public class StringOption: Option
    {
        private string? _value;

        /// <summary>
        /// Creates a new <see cref="StringOption"/> with the specified <paramref name="code"/> and <paramref name="data"/>.
        /// </summary>
        public StringOption(
            DhcpOptionCode code,
            byte[] data) : base(code, data)
        {
        }

        /// <summary>
        /// Creates a new <see cref="StringOption"/> with the specified <paramref name="code"/> and <paramref name="value"/>.
        /// </summary>
        public StringOption(
            DhcpOptionCode code,
            string value) : this(code, Converter.GetBytes(value))
        {
            _value = value;
        }

        /// <summary>
        /// Gets the <see cref="string"/> set for this DHCP option.
        /// </summary>
        public string Deserialize()
        {
            return _value ??= Converter.GetString(Data);
        }

        internal static bool IsKnownOption(byte code) => IsKnownOption((DhcpOptionCode)code);

        internal static bool IsKnownOption(DhcpOptionCode code)
        {
            return code switch
            {
                DhcpOptionCode.HostName => true,
                _ => false
            };
        }

        /// <inheritdoc />
        public override string ToString() => ToString(Deserialize());
    }
}
