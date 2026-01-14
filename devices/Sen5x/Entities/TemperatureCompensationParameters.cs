// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;
using System.Buffers.Binary;
using UnitsNet;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// Represents the temperature compensation parameters.
    /// </summary>
    public class TemperatureCompensationParameters : AbstractReadWriteEntity
    {
        internal override int ByteCount => 9;

        internal override void FromSpanByte(Span<byte> data)
        {
            TemperatureOffset = Temperature.FromDegreesCelsius(BinaryPrimitives.ReadInt16BigEndian(data) / 200.0);
            NormalizedTemperatureOffsetSlope = BinaryPrimitives.ReadInt16BigEndian(data.Slice(3)) / 10000.0;
            TimeConstant = TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6)));
        }

        internal override void ToSpanByte(Span<byte> data)
        {
            BinaryPrimitives.WriteInt16BigEndian(data, (short)(TemperatureOffset.DegreesCelsius * 200.0));
            BinaryPrimitives.WriteInt16BigEndian(data.Slice(3), (short)(NormalizedTemperatureOffsetSlope * 10000.0));
            BinaryPrimitives.WriteUInt16BigEndian(data.Slice(6), (ushort)TimeConstant.TotalSeconds);
        }

        /// <summary>
        /// Gets or sets the temperature offset [°C] (default value: 0).
        /// </summary>
        public Temperature TemperatureOffset { get; set; }

        /// <summary>
        /// Gets or sets the normalized temperature offset slope (default value: 0).
        /// </summary>
        public double NormalizedTemperatureOffsetSlope { get; set; }

        /// <summary>
        /// Gets or sets the time constant in seconds (default value: 0).
        /// </summary>
        public TimeSpan TimeConstant { get; set; }
    }
}
