// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;
using System;

namespace Iot.Device.DhcpServer.Options
{
    /// <summary>
    /// Represents a DHCP option with a <see cref="TimeSpan"/> value.
    /// </summary>
    /// <remarks>The <see cref="TimeSpan"/> is serialized as positive seconds.</remarks>
    public class TimeSpanOption: Option
    {
        private bool _deserialized;
        private TimeSpan _value;

        /// <summary>
        /// Creates a new <see cref="TimeSpanOption"/> with the specified <paramref name="code"/> and <paramref name="data"/>.
        /// </summary>
        public TimeSpanOption(
            DhcpOptionCode code,
            byte[] data) : base(code,
                                data) { }

        /// <summary>
        /// Creates a new <see cref="TimeSpanOption"/> with the specified <paramref name="code"/> and <paramref name="value"/>.
        /// </summary>
        public TimeSpanOption(
            DhcpOptionCode code,
            TimeSpan value) : this(code,
                                   Converter.GetBytes(value))
        {
            _deserialized = true;
            _value = value;
        }

        /// <summary>
        /// Gets the <see cref="TimeSpan"/> set for this DHCP option.
        /// </summary>
        public TimeSpan Deserialize()
        {
            if (!_deserialized)
            {
                _deserialized = true;
                _value = Converter.GetTimeSpan(Data);
            }

            return _value;
        }

        internal static bool IsKnownOption(byte code) => IsKnownOption((DhcpOptionCode)code);

        internal static bool IsKnownOption(DhcpOptionCode code)
        {
            return code switch
            {
                DhcpOptionCode.LeaseTime => true,
                DhcpOptionCode.RebindingTime => true,
                DhcpOptionCode.RenewalTime => true,
                _ => false
            };
        }

        /// <inheritdoc />
        public override string ToString() => ToString(Deserialize());
    }
}
