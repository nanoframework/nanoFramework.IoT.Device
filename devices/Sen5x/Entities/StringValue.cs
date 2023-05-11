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
        private int _maxLength;

        internal StringValue(int maxLength = 32)
        {
            // Internal constructor, because this is a read-only entity.
            _maxLength = maxLength;
        }

        internal override int ByteCount => _maxLength / 2 * 3;

        internal override void FromSpanByte(SpanByte data)
        {
            Text = string.Empty;
            for (int i = 0; i < data.Length; i += 3)
            {
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

                // skip 3rd byte, which contains the checksum
            }
        }

        public string Text { get; protected set; }
    }
}
