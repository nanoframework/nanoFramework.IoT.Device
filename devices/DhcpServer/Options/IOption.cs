// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.DhcpServer.Enums;

namespace Iot.Device.DhcpServer.Options
{
    /// <summary>
    /// Represents a DHCP option.
    /// </summary>
    public interface IOption
    {
        /// <summary>
        /// Gets the <see cref="DhcpOptionCode"/>.
        /// </summary>
        public DhcpOptionCode Code { get; }

        /// <summary>
        /// Gets the data associated with the DHCP option.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Gets the length of <see cref="Data"/>.
        /// </summary>
        public byte Length { get; }

        /// <summary>
        /// Gets the length of the serialized <see cref="IOption"/>.
        /// </summary>
        public ushort OptionLength { get; }

        /// <summary>
        /// Converts the <see cref="IOption"/> to a <see cref="T:byte[]"/>.
        /// </summary>
        /// <returns>A <see cref="T:byte[]"/> representing the <see cref="IOption"/>.</returns>
        public byte[] GetBytes();

        /// <summary>
        /// Gets the string representation of this <see cref="IOption"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representing the <see cref="IOption"/>.</returns>
        public string ToString();
    }
}
