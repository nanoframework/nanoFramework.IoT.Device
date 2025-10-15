// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Represents a DNS answer.
    /// </summary>
    internal class DnsAnswer
    {
        /// <summary>
        /// The length of a DNS answer in bytes.
        /// </summary>
        public const byte Length = 16;

        /// <summary>
        /// DNS compression pointer flag (top two bits set to indicate a pointer).
        /// </summary>
        private const ushort DnsCompressionPointerFlag = 0xC000;

        /// <summary>
        /// Default TTL for DNS answers (in seconds) - 30 minutes to match the C implementation.
        /// </summary>
        private const uint DefaultTtl = 1800;

        /// <summary>
        /// Gets the length of the address in bytes. For IPv4 addresses, this is always 4.
        /// </summary>
        public static ushort AddressLength => 4;

        /// <summary>
        /// Gets or sets the offset of the name in the DNS message.
        /// This is used to create a compression pointer to the original question.
        /// </summary>
        public ushort NameOffset { get; set; }

        /// <summary>
        /// Gets or sets the type of the DNS answer.
        /// </summary>
        public ushort Type { get; set; }

        /// <summary>
        /// Gets or sets the class of the DNS answer.
        /// </summary>
        public ushort Class { get; set; }

        /// <summary>
        /// Gets or sets the time to live (TTL) of the DNS answer, in seconds.
        /// </summary>
        /// <remarks>This value indicates how long the answer can be cached by clients and resolvers. The default is 1800 seconds (30 minutes).</remarks>
        public uint TTL { get; set; } = DefaultTtl;

        /// <summary>
        /// Gets or sets the IP address associated with the DNS answer.
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        /// Gets the byte array representation of the DNS answer.
        /// </summary>
        /// <returns>A byte array containing the DNS answer data.</returns>
        public byte[] GetBytes()
        {
            byte[] result = new byte[Length];
            int position = 0;

            // Create proper compression pointer (0xC000 | offset)
            ushort compressionPointer = (ushort)(DnsCompressionPointerFlag | NameOffset);

            Logger.GlobalLogger.LogDebug("DNS Answer - Creating pointer 0x{0:X4} (flag=0x{1:X2}, offset=0x{2:X4})", compressionPointer, DnsCompressionPointerFlag, NameOffset);

            // Write each field in network byte order (big-endian)
            ByteHelper.WriteUInt16NetworkOrder(compressionPointer, result, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt16NetworkOrder(Type, result, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt16NetworkOrder(Class, result, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt32NetworkOrder(TTL, result, position);
            position += sizeof(uint);

            ByteHelper.WriteUInt16NetworkOrder(AddressLength, result, position);
            position += sizeof(ushort);

            Array.Copy(Address.GetAddressBytes(), 0, result, position, AddressLength);

            return result;
        }
    }
}
