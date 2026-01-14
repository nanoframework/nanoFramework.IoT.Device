// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// Represents a null-terminated string value read from the sensor.
    /// </summary>
    internal class StringValue : AbstractReadEntity
    {
        private readonly int _maxLength;

        internal StringValue(int maxLength = 32)
        {
            // Internal constructor, because this is a read-only entity.
            _maxLength = maxLength;
        }

        internal override int ByteCount => _maxLength / 2 * 3; // To account for checksum bytes.

        internal override void FromSpanByte(Span<byte> data)
        {
            // We don't use Encoding.UTF8.GetString(), because:
            // - We need to skip every 3rd byte, which is a checksum byte, so we'd need to make a
            //   copy of the data and filter every 3rd byte specifically for this purpose.
            // - It would add another nuget dependency for only this.
            // - The sensor does not support any special characters anyway.
            Text = string.Empty;
            for (int i = 0; i < data.Length; i += 3)
            {
                // Any null byte ends the string.
                if (data[i] == '\0')
                {
                    break;
                }

                Text += (char)data[i];

                if (data[i + 1] == '\0')
                {
                    break;
                }

                Text += (char)data[i + 1];

                // Skip 3rd byte, which contains the checksum.
            }
        }

        public string Text { get; protected set; }
    }
}
