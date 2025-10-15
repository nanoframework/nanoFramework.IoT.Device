// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Logging;
using System;
using System.Collections;

namespace Iot.Device.DnsServer
{
    /// <summary>
    /// Represents a DNS reply.
    /// </summary>
    internal class DnsReply
    {
        /// <summary>
        /// DNS flag mask for the response flag (bit 15).
        /// </summary>
        private const ushort _DnsFlagResponseMask = 0x8000;

        /// <summary>
        /// Hashtable of DNS entries.
        /// </summary>
        private readonly Hashtable _dnsEntries;

        private readonly byte[] _requestData;

        /// <summary>
        /// Gets the DNS header.
        /// </summary>
        public DnsHeader Header { get; private set; }

        /// <summary>
        /// Gets the question section from the DNS request.
        /// </summary>
        /// <remarks>Currently supports only one question.</remarks>
        public DnsQuestion[] DnsQuestions { get; internal set; }

        /// <summary>
        /// Gets the DNS answer for the DNS reply.
        /// </summary>
        public DnsAnswer[] Answers { get; internal set; }

        /// <summary>
        /// Creates a new instance of the <see cref="DnsReply"/> class with the specified DNS request data and DNS entries.
        /// </summary>
        /// <param name="requestData">The DNS request data as a byte array.</param>
        /// <param name="dnsEntries">The hashtable of DNS entries to use for generating answers.</param>
        public DnsReply(
            byte[] requestData,
            Hashtable dnsEntries)
        {
            _requestData = requestData;

            _dnsEntries = dnsEntries;
        }

        public bool ParseRequest()
        {
            // parse DNS header
            Header = new DnsHeader(_requestData);

            Logger.GlobalLogger.LogDebug("DNS Header - ID: {0}, Flags: {1}, QDCount: {2}", Header.Id, Header.Flags, Header.QDCount);

            // only process standard queries
            if ((Header.Flags & DnsHeader.DnsFlagOpCodeMask) != 0)
            {
                Logger.GlobalLogger.LogWarning("Unsupported DNS operation code.");
                return false;
            }

            // only process requests with a single question
            if (Header.QDCount == 0)
            {
                Logger.GlobalLogger.LogWarning("DNS request contains no questions.");
                return false;
            }
            else if (Header.QDCount > 1)
            {
                Logger.GlobalLogger.LogWarning("DNS request contains multiple questions. Only single question requests are supported.");
                return false;
            }

            // set question response flag
            Header.Flags |= _DnsFlagResponseMask;

            // adjust answer count with same as question count
            Header.ANCount = Header.QDCount;

            DnsQuestions = new DnsQuestion[] { new DnsQuestion(_requestData, DnsHeader.Length) };
            Answers = new DnsAnswer[Header.QDCount];

            if (!DnsQuestions[0].TryParseQuestion(_dnsEntries, ref Answers[0]))
            {
                Logger.GlobalLogger.LogWarning("Failed to parse DNS question.");

                return false;
            }

            return true;
        }



        /// <summary>
        /// Gets the byte array representation of the DNS reply.
        /// </summary>
        /// <returns>A byte array containing the DNS reply data.</returns>
        public byte[] GetBytes()
        {
            // Calculate the total size needed for the DNS reply
            int totalSize = DnsHeader.Length + DnsQuestions[0].Length + DnsAnswer.Length;

            // build reply from header and answers
            byte[] replyData = new byte[totalSize];

            // copy header with modified flags and answer count
            Array.Copy(Header.GetBytes(), 0, replyData, 0, DnsHeader.Length);

            Logger.GlobalLogger.LogDebug("DNS Reply - Header: ID={0}, Flags=0x{1:X4}, QD={2}, AN={3}", Header.Id, Header.Flags, Header.QDCount, Header.ANCount);

            int position = DnsHeader.Length;

            // copy questions section from the original request
            Array.Copy(DnsQuestions[0].GetBytes(), 0, replyData, position, DnsQuestions[0].Length);

            Logger.GlobalLogger.LogDebug("DNS Reply - Added questions section, {0} bytes", DnsQuestions.Length);

            // move position forward
            position += DnsQuestions[0].Length;

            // copy answer
            Array.Copy(Answers[0].GetBytes(), 0, replyData, position, DnsAnswer.Length);

            Logger.GlobalLogger.LogDebug("DNS Reply - Added answer: Name ptr=0x{0:X4}, Type={1}, IP={2}", Answers[0].NameOffset, Answers[0].Type, Answers[0].Address);

            Logger.GlobalLogger.LogDebug("DNS Reply total size: {0} bytes", replyData.Length);

            return replyData;
        }
    }
}
