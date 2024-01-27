// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;
using System.Buffers.Binary;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// Represents the warm start parameters.
    /// </summary>
    public class WarmStartParameters : AbstractReadWriteEntity
    {
        internal override int ByteCount => 3;

        internal override void FromSpanByte(SpanByte data)
        {
            WarmStartBehavior = BinaryPrimitives.ReadUInt16BigEndian(data);
        }

        internal override void ToSpanByte(SpanByte data)
        {
            BinaryPrimitives.WriteUInt16BigEndian(data, WarmStartBehavior);
        }

        /// <summary>
        /// Gets or sets the warm start behavior as a value in the range from 0 (cold start, default value) to 65535 (warm start).
        /// </summary>
        public ushort WarmStartBehavior { get; set; }
    }
}
