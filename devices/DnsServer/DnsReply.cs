// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        private Hashtable _dnsEntries;

        private byte[] _requestData;

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

            Logger.Debug($"DNS Header - ID: {Header.Id}, Flags: {Header.Flags}, QDCount: {Header.QDCount}");

            // only process standard queries
            if ((Header.Flags & DnsHeader.DnsFlagOpCodeMask) != 0)
            {
                Logger.Warning("Unsupported DNS operation code.");
                return false;
            }

            // only process requests with a single question
            if (Header.QDCount == 0)
            {
                Logger.Warning("DNS request contains no questions.");
                return false;
            }
            else if (Header.QDCount > 1)
            {
                Logger.Warning("DNS request contains multiple questions. Only single question requests are supported.");
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
                Logger.Warning("Failed to parse DNS question.");

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

            Logger.Debug($"DNS Reply - Header: ID={Header.Id}, Flags=0x{Header.Flags:X4}, QD={Header.QDCount}, AN={Header.ANCount}");

            int position = DnsHeader.Length;

            // copy questions section from the original request
            Array.Copy(DnsQuestions[0].GetBytes(), 0, replyData, position, DnsQuestions[0].Length);

            Logger.Debug($"DNS Reply - Added questions section, {DnsQuestions.Length} bytes");

            // move position forward
            position += DnsQuestions[0].Length;

            // copy answer
            Array.Copy(Answers[0].GetBytes(), 0, replyData, position, DnsAnswer.Length);

            Logger.Debug($"DNS Reply - Added answer: Name ptr=0x{Answers[0].NameOffset:X4}, Type={Answers[0].Type}, IP={Answers[0].Address}");

            //// Dump the first few bytes of the reply for debugging
            //if (replyData.Length >= 16)
            //{
            //    Logger.Debug($"DNS Reply first 16 bytes: {BytesToHexString(replyData, 0, 16)}");
            //}

            Logger.Debug($"DNS Reply total size: {replyData.Length} bytes");

            return replyData;
        }

        ///// <summary>
        ///// Converts a byte array to a hex string for debugging.
        ///// </summary>
        //private static string BytesToHexString(byte[] bytes, int offset, int length)
        //{
        //    if (bytes == null || bytes.Length == 0 || offset < 0 || length <= 0)
        //        return string.Empty;

        //    // Make sure we don't go beyond the array bounds
        //    length = Math.Min(length, bytes.Length - offset);

        //    StringBuilder sb = new StringBuilder(length * 3);
        //    for (int i = 0; i < length; i++)
        //    {
        //        sb.Append(bytes[offset + i].ToString("X2"));

        //        if (i < length - 1)
        //            sb.Append(' ');
        //    }

        //    return sb.ToString();
        //}
    }
}
