// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Iot.Device.Card.CreditCardProcessing
{
    /// <summary>
    /// A simple Basic Encoding Rules (defined in ISO/IEC 8825–1) decoder
    /// </summary>
    internal class BerSplitter
    {
        /// <summary>
        /// A list of Tag
        /// </summary>
        public ListTag Tags { get; set; }

        /// <summary>
        /// Constructor taking a BER encoded array
        /// </summary>
        /// <param name="toSplit">The byte array to be decoded</param>
        public BerSplitter(SpanByte toSplit)
        {
            Tags = new ListTag();
            int index = 0;
            while ((index < toSplit.Length) && (toSplit[index] != 0x00))
            {
                try
                {
                    var resTag = DecodeTag(toSplit.Slice(index));
                    var tagNumber = resTag.TagNumber;
                    // Need to move index depending on how many has been read
                    index += resTag.NumberElements;
                    var resSize = DecodeSize(toSplit.Slice(index));
                    var data = new byte[resSize.Size];
                    var elem = new Tag(
                        tagNumber,
                        data);
                    Tags.Add(elem);
                    index += resSize.NumBytes;
                    toSplit.Slice(index, resSize.Size).CopyTo(elem.Data);
                    index += resSize.Size;

                }
                catch (Exception ex) when (ex is ArgumentOutOfRangeException || ex is OverflowException)
                {
                    // We may have a non supported Tag
                    break;
                }
            }
        }

        private (uint TagNumber, byte NumberElements) DecodeTag(SpanByte toSplit)
        {
            uint tagValue = toSplit[0];
            byte index = 1;
            // check if single or double triple or quadruple element
            if ((toSplit[0] & 0b0001_1111) == 0b0001_1111)
            {
                do
                {
                    tagValue = tagValue << 8 | toSplit[index];
                }
                while ((toSplit[index++] & 0x80) == 0x80);

            }

            return (tagValue, index);
        }

        private (int Size, byte NumBytes) DecodeSize(SpanByte toSplit)
        {
            // Case infinite
            if (toSplit[0] == 0b1000_0000)
            {
                return (-1, 1);
            }

            // Check how many bytes
            if ((toSplit[0] & 0b1000_0000) == 0b1000_0000)
            {
                // multiple bytes
                var numBytes = toSplit[0] & 0b0111_1111;
                int size = 0;
                for (int i = 0; i < numBytes; i++)
                {
                    size = (size << 8) + toSplit[1 + i];
                }

                return (size, (byte)(numBytes + 1));
            }

            return (toSplit[0], 1);
        }
    }
}
