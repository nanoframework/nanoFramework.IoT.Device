// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;
using System.Buffers.Binary;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// Represents the automatic cleaning interval configuration.
    /// </summary>
    public class AutoCleaningIntervalParameters : AbstractReadWriteEntity
    {
        internal override int ByteCount => 6;

        internal override void FromSpanByte(SpanByte data)
        {
            SpanByte buf = new byte[4];
            buf[0] = data[0];
            buf[1] = data[1];
            buf[2] = data[3];
            buf[3] = data[4];
            AutoCleaningInterval = TimeSpan.FromSeconds(BinaryPrimitives.ReadInt32BigEndian(buf));
        }

        internal override void ToSpanByte(SpanByte data)
        {
            SpanByte buf = new byte[4];
            BinaryPrimitives.WriteInt32BigEndian(buf, (int)AutoCleaningInterval.TotalSeconds);
            data[0] = buf[0];
            data[1] = buf[1];
            data[3] = buf[2];
            data[4] = buf[3];
        }

        /// <summary>
        /// Gets or sets the Auto Cleaning Interval [s].
        /// </summary>
        public TimeSpan AutoCleaningInterval { get; set; }
    }
}
