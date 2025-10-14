// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Net;
using System.Text;

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Represents a DNS question.
    /// </summary>
    internal class DnsQuestion
    {
        /// <summary>
        /// DNS question type for A (IPv4 address) records.
        /// </summary>
        public const ushort QuestionTypeA = 0x0001;

        /// <summary>
        /// DNS compression pointer flag. 
        /// When the two highest bits of a label length byte are set (0xC0), 
        /// it indicates that this is a pointer to another location in the message.
        /// </summary>
        private const byte _DnsCompressionPointerFlag = 0xC0;

        /// <summary>
        /// Wildcard character for DNS entries, used to match any subdomain.
        /// </summary>
        private const char _WildcardCharacter = '*';

        /// <summary>
        /// The length of a DNS question in bytes.
        /// </summary>
        public ushort Length { get; }

        public byte[] Name { get; }

        /// <summary>
        /// Gets the type of the DNS question.
        /// </summary>
        public ushort Type { get; }

        /// <summary>
        /// Gets the class of the DNS question.
        /// </summary>
        public ushort Class { get; }


        /// <summary>
        /// Gets the offset of the question name in the DNS message.
        /// </summary>
        public ushort QuestionOffset { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DnsQuestion"/> class by parsing the given byte array starting at the specified offset.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        public DnsQuestion(byte[] data, int offset)
        {
            int terminatorIndex = -1;

            // find the null terminator for the name
            for (int i = offset; i < data.Length; i++)
            {
                if (data[i] == 0)
                {
                    terminatorIndex = i;
                    break;
                }
            }

            if (terminatorIndex < 0)
            {
                throw new ArgumentException();
            }

            Name = new byte[terminatorIndex - offset + 1];

            // copy over the name including the null terminator
            Array.Copy(data, offset, Name, 0, terminatorIndex - offset + 1);

            // store the question offset
            QuestionOffset = (ushort)offset;

            // compute length
            Length = (ushort)(terminatorIndex - offset + 1);
            int position = offset + Length;

            Type = ByteHelper.ReadUInt16NetworkOrder(data, position);

            Length += sizeof(ushort);

            Class = ByteHelper.ReadUInt16NetworkOrder(data, position + sizeof(ushort));

            Length += sizeof(ushort);
        }

        /// <summary>
        /// Tries to parse a DNS question from the given byte array starting at the specified offset.
        /// </summary>
        /// <returns><see langword="true"/> if the question was parsed successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryParseQuestion(Hashtable dnsEntries, ref DnsAnswer dnsAnswer)
        {
            // parse name
            ushort nameLength = 0;
            string name = ParseName(ref nameLength);

            if (string.IsNullOrEmpty(name))
            {
                Logger.Warning("Failed to parse DNS question name.");

                return false;
            }

            Logger.Debug($"Parsed DNS question: Name={name}, Type={Type}, Class={Class} (name offset={QuestionOffset}, length={nameLength})");

            // For captive portal, we'll handle A record types
            if (Type != QuestionTypeA)
            {
                Logger.Warning($"Unsupported DNS question type: {Type}. Only A (IPv4 address) records are supported.");

                return false;
            }

            // Try to find matching DNS entry
            if (TryFindDnsEntry(
                name,
                dnsEntries,
                out IPAddress ipAddress))
            {
                // Create answer with proper name pointer to the original question name
                dnsAnswer = new DnsAnswer()
                {
                    NameOffset = QuestionOffset,
                    Type = Type,
                    Class = Class,
                    Address = ipAddress,
                    TTL = 300
                };

                Logger.Debug($"Generated DNS answer for name: {name}, Type: {Type}, Address: {ipAddress}, pointer offset: 0x{QuestionOffset:X4}");
            }
            else
            {
                Logger.Warning($"No DNS entry found for name: {name}");

                return false;
            }

            // Successfully parsed and processed the question
            return true;
        }

        /// <summary>
        /// Parse a DNS name from the raw format to a regular dot-separated domain name.
        /// </summary>
        /// <param name="data">The byte array containing the DNS message data.</param>
        /// <param name="offset">The offset in the data array where the name starts.</param>
        /// <param name="nameLength">The length of the name in the <paramref name="data"/> buffer, in bytes.</param>
        /// <returns>The parsed domain name or empty string if parsing failed.</returns>
        private string ParseName(ref ushort nameLength)
        {
            StringBuilder nameBuilder = new StringBuilder();
            int position = 0;
            int startPosition = 0;

            try
            {
                while (position < Name.Length)
                {
                    // Get the length of this label
                    int labelLength = Name[position++];

                    // If length is 0, we've reached the end of the name
                    if (labelLength == 0)
                    {
                        // Include the terminating zero byte in the total length
                        nameLength = (ushort)(position - startPosition);
                        break;
                    }

                    // Check for DNS name compression (pointer)
                    if ((labelLength & _DnsCompressionPointerFlag) == _DnsCompressionPointerFlag)
                    {
                        // This is a pointer (first two bits are set)
                        Logger.Warning("DNS name compression (pointers) not supported in this simplified implementation");

                        nameLength = 0;

                        return string.Empty;
                    }

                    // Make sure we have enough bytes for this label
                    if (position + labelLength > Name.Length)
                    {
                        Logger.Warning("DNS label exceeds packet boundary");

                        nameLength = 0;

                        return string.Empty;
                    }

                    // Add a dot if this isn't the first label
                    if (nameBuilder.Length > 0)
                    {
                        nameBuilder.Append('.');
                    }

                    // Append this label to our result
                    nameBuilder.Append(System.Text.Encoding.UTF8.GetString(Name, position, labelLength));

                    // Move past this label
                    position += labelLength;
                }

                return nameBuilder.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception parsing DNS name: " + ex.Message, ex);

                nameLength = 0;

                return string.Empty;
            }
        }


        /// <summary>
        /// Tries to find a matching DNS entry for the given name.
        /// </summary>
        /// <param name="name">The domain name to look up.</param>
        /// <param name="ipAddress">The IP address if found.</param>
        /// <returns><see langword="true"/> if a match was found; otherwise, <see langword="false"/>.</returns>
        private bool TryFindDnsEntry(
            string name,
            Hashtable dnsEntries,
            out IPAddress ipAddress)
        {
            ipAddress = null;

            // Try exact match first
            if (dnsEntries.Contains(name))
            {
                ipAddress = (IPAddress)dnsEntries[name];

                return true;
            }

            // Try wildcard match
            foreach (string key in dnsEntries.Keys)
            {
                // Check if this is a wildcard entry (starts with *)
                if (key.StartsWith(_WildcardCharacter.ToString()))
                {
                    // Remove the *
                    string suffix = key.Substring(1);

                    if (name.EndsWith(suffix))
                    {
                        ipAddress = (IPAddress)dnsEntries[key];

                        Logger.Debug($"Wildcard match: '{name}' matches pattern '{key}'");

                        return true;
                    }
                }
            }

            return false;
        }

        /// Returns the byte array representation of this DNS question, suitable for network transmission.
        /// </summary>
        /// <param name="none"></param>
        /// <returns>
        /// A <see cref="byte"/> array containing the DNS question in wire format.
        /// </returns>
        internal byte[] GetBytes()
        {
            byte[] buffer = new byte[Length];

            Array.Copy(Name, 0, buffer, 0, Name.Length);
            int position = Name.Length;

            ByteHelper.WriteUInt16NetworkOrder(Type, buffer, position);
            position += sizeof(ushort);

            ByteHelper.WriteUInt16NetworkOrder(Class, buffer, position);

            return buffer;
        }
    }
}
