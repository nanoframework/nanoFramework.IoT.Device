// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

namespace Iot.Device.Ndef
{
    /// <summary>
    /// Create a NDEF = NFC Data Exchange Format Message class
    /// </summary>
    public class NdefMessage
    {
        /// <summary>
        /// Associated with the GeneralPurposeByteConsitions, it tells if a sector is read/write and a valid
        /// NDEF sector
        /// </summary>
        public const byte GeneralPurposeByteNdefVersion = 0b0100_0000;

        /// <summary>
        /// From a raw message, find the start and stop of an NDEF message
        /// </summary>
        /// <param name="toExtract">The byte array where the message is</param>
        /// <returns>The start and end position</returns>
        public static Doublet GetStartSizeNdef(SpanByte toExtract)
        {
            int idx = 0;
            // Check if we have 0x03 so it's a possible, NDEF Entry
            while (idx < toExtract.Length)
            {
                if (toExtract[idx++] == 0x03)
                {
                    break;
                }
            }

            if (idx == toExtract.Length)
            {
                return new Doublet(-1, -1);
            }

            // Now check the size. If 0xFF then encoding is on 3 bytes otherwise just one
            int size = toExtract[idx++];
            if (idx == toExtract.Length)
            {
                return new Doublet(idx, -1);
            }

            if (size == 0xFF)
            {
                if (idx + 2 >= toExtract.Length)
                {
                    return new Doublet(idx, -1);
                }

                size = (toExtract[idx++] << 8) + toExtract[idx++];
            }

            return new Doublet(idx, size);
        }

        /// <summary>
        /// Extract an NDEF message from a raw byte array
        /// </summary>
        /// <param name="toExtract">The byte array where the message is</param>
        /// <returns>A byte array containing the message itself</returns>
        public static byte[]? ExtractMessage(SpanByte toExtract)
        {
            var doublet = GetStartSizeNdef(toExtract);
            int idx = doublet.Start;
            int size = doublet.Size;
            // Finally check that the end terminator TLV is 0xFE
            bool isRealEnd = toExtract[idx + size] == 0xFE;
            if (!isRealEnd)
            {
                return new byte[0];
            }

            // Now we have the real size and we can extract the real buffer
            byte[] toReturn = new byte[size];
            toExtract.Slice(idx, size).CopyTo(toReturn);

            return toReturn;
        }

        /// <summary>
        /// List of all NDEF Records
        /// </summary>
        public ListNdefRecord Records { get; set; } = new ListNdefRecord();

        /// <summary>
        /// Create an empty NDEF Message
        /// </summary>
        public NdefMessage()
        {
        }

        /// <summary>
        /// Create NDEF Message from a span of bytes
        /// </summary>
        /// <param name="message">the message in span of bytes</param>
        public NdefMessage(SpanByte message)
        {
            int idxMessage = 0;
            while (idxMessage < message.Length)
            {
                var ndefrec = new NdefRecord(message.Slice(idxMessage));
                Records.Add(ndefrec);
                idxMessage += ndefrec.Length;
            }
        }

        /// <summary>
        /// Get the length of the message
        /// </summary>
        public int Length
        {
            get
            {
                // Ogiginal: => Records.Select(m => m.Length).Sum();
                int sum = 0;
                for (int i = 0; i < Records.Count; i++)
                {
                    sum += Records[i].Length;
                }

                return sum;
            }
        }

        /// <summary>
        /// Serialize the message in a span of bytes
        /// </summary>
        /// <param name="messageSerialized">Span of bytes for the serialized message</param>
        public void Serialize(SpanByte messageSerialized)
        {
            if (messageSerialized.Length < Length)
            {
                throw new ArgumentException($"Span of bytes needs to be at least as large as the Message total length");
            }

            if (Records.Count == 0)
            {
                return;
            }

            // Make sure we set correctly the Begin and End message flags
            Records[0].Header.MessageFlag |= MessageFlag.MessageBegin;
            Records[0].Header.MessageFlag &= ~MessageFlag.MessageEnd;
            Records[Records.Count - 1].Header.MessageFlag |= MessageFlag.MessageEnd;

            int idx = 0;
            for (int i = 0; i < Records.Count; i++)
            {
                if ((i != 0) && (i != (Records.Count - 1)))
                {
                    Records[i].Header.MessageFlag &= ~MessageFlag.MessageBegin;
                    Records[i].Header.MessageFlag &= ~MessageFlag.MessageEnd;
                }

                Records[i].Serialize(messageSerialized.Slice(idx));
                idx += Records[i].Length;
            }
        }
    }
}
