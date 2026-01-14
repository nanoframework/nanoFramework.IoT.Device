// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// Represents the VOC algorithm state.
    /// </summary>
    public class VocAlgorithmState : AbstractReadWriteEntity
    {
        internal override int ByteCount => 12;

        internal override void FromSpanByte(Span<byte> data)
        {
            State = new byte[8];
            State[0] = data[0];
            State[1] = data[1];
            State[2] = data[3];
            State[3] = data[4];
            State[4] = data[6];
            State[5] = data[7];
            State[6] = data[9];
            State[7] = data[10];
        }

        internal override void ToSpanByte(Span<byte> data)
        {
            data[0] = State[0];
            data[1] = State[1];
            data[3] = State[2];
            data[4] = State[3];
            data[6] = State[4];
            data[7] = State[5];
            data[9] = State[6];
            data[10] = State[7];
        }

        /// <summary>
        /// Gets or sets the VOC algorithm state represented in an 8 byte array.
        /// </summary>
        public byte[] State { get; set; }
    }
}
