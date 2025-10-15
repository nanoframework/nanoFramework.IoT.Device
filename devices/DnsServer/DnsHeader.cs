// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Represents a DNS header.
    /// </summary>
    internal class DnsHeader
    {
        /// <summary>
        /// DNS flag mask for the operation code (bits 11-14).
        /// </summary>
        public const ushort DnsFlagOpCodeMask = 0x7800;

        /// <summary>
        /// The length of a DNS header in bytes.
        /// </summary>
        public const byte Length = 12;

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        public ushort Flags { get; internal set; }

        /// <summary>
        /// Gets the question count.
        /// </summary>
        public ushort QDCount { get; }

        /// <summary>
        /// Gets or sets the answer record count.
        /// </summary>
        public ushort ANCount { get; internal set; }

        /// <summary>
        /// Gets the authority record count.
        /// </summary>
        public ushort NSCount { get; }

        /// <summary>
        /// Gets the additional record count.
        /// </summary>
        public ushort ARCount { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DnsHeader"/> class by parsing the given byte array.
        /// </summary>
        /// <param name="data">The byte array containing the DNS header data.</param>
        public DnsHeader(byte[] data)
        {
            if (data.Length < Length)
            {
                throw new ArgumentException();
            }

            int position = 0;

            Id = ByteHelper.ReadUInt16NetworkOrder(data, position);
            position += sizeof(ushort);

            Flags = ByteHelper.ReadUInt16NetworkOrder(data, position);
            position += sizeof(ushort);

            QDCount = ByteHelper.ReadUInt16NetworkOrder(data, position);
            position += sizeof(ushort);

            ANCount = ByteHelper.ReadUInt16NetworkOrder(data, position);
            position += sizeof(ushort);

            NSCount = ByteHelper.ReadUInt16NetworkOrder(data, position);
            position += sizeof(ushort);

            ARCount = ByteHelper.ReadUInt16NetworkOrder(data, position);
        }

        /// <summary>
        /// Converts the DNS header to a byte array.
        /// </summary>
        /// <returns>A byte array representing the DNS header.</returns>
        public byte[] GetBytes()
        {
            byte[] data = new byte[Length];
            int position = 0;

            // Write each ushort field to the array using network byte order
            ByteHelper.WriteUInt16NetworkOrder(Id, data, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt16NetworkOrder(Flags, data, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt16NetworkOrder(QDCount, data, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt16NetworkOrder(ANCount, data, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt16NetworkOrder(NSCount, data, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt16NetworkOrder(ARCount, data, position);

            return data;
        }
    }
}
