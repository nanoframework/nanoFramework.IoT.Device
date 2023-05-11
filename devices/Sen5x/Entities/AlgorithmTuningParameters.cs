// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;
using System.Buffers.Binary;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// Represents the algorithm tuning parameters for either VOC or NOx.
    /// </summary>
    public class AlgorithmTuningParameters : AbstractReadWriteEntity
    {
        internal override int ByteCount => 18;

        internal override void FromSpanByte(SpanByte data)
        {
            IndexOffset = BinaryPrimitives.ReadInt16BigEndian(data);
            LearningTimeOffset = TimeSpan.FromHours(BinaryPrimitives.ReadInt16BigEndian(data.Slice(3)));
            LearningTimeGain = TimeSpan.FromHours(BinaryPrimitives.ReadInt16BigEndian(data.Slice(6)));
            GatingMaxDuration = TimeSpan.FromMinutes(BinaryPrimitives.ReadInt16BigEndian(data.Slice(9)));
            StdInitial = BinaryPrimitives.ReadInt16BigEndian(data.Slice(12));
            GainFactor = BinaryPrimitives.ReadInt16BigEndian(data.Slice(15));
        }

        internal override void ToSpanByte(SpanByte data)
        {
            BinaryPrimitives.WriteInt16BigEndian(data, IndexOffset);
            BinaryPrimitives.WriteInt16BigEndian(data.Slice(3), (short)LearningTimeOffset.TotalHours);
            BinaryPrimitives.WriteInt16BigEndian(data.Slice(6), (short)LearningTimeGain.TotalHours);
            BinaryPrimitives.WriteInt16BigEndian(data.Slice(9), (short)GatingMaxDuration.TotalMinutes);
            BinaryPrimitives.WriteInt16BigEndian(data.Slice(12), StdInitial);
            BinaryPrimitives.WriteInt16BigEndian(data.Slice(15), GainFactor);
        }

        /// <summary>
        /// Gets or sets the index offset.
        /// </summary>
        public short IndexOffset { get; set; }

        /// <summary>
        /// Gets or sets the Learning Time Offset Hours. Note that the smallest counted unit is hours.
        /// </summary>
        public TimeSpan LearningTimeOffset { get; set; }

        /// <summary>
        /// Gets or sets the Learning Time Gain Hours. Note that the smallest counted unit is hours.
        /// </summary>
        public TimeSpan LearningTimeGain { get; set; }

        /// <summary>
        /// Gets or sets the Gating Max Duration Minutes. Note that the smallest counted unit is minutes.
        /// </summary>
        public TimeSpan GatingMaxDuration { get; set; }

        /// <summary>
        /// Gets or sets the Std Initial.
        /// </summary>
        public short StdInitial { get; set; }

        /// <summary>
        /// Gets or sets the Gain Factor.
        /// </summary>
        public short GainFactor { get; set; }
    }
}
