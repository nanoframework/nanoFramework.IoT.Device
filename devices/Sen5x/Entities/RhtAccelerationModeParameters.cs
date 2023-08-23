// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;
using System.Buffers.Binary;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// Represents the configurable settings for RH/T.
    /// </summary>
    public class RhtAccelerationModeParameters : AbstractReadWriteEntity
    {
        internal override int ByteCount => 3;

        internal override void FromSpanByte(SpanByte data)
        {
            Mode = (RhtAccelerationModes)BinaryPrimitives.ReadUInt16BigEndian(data);
        }

        internal override void ToSpanByte(SpanByte data)
        {
            BinaryPrimitives.WriteUInt16BigEndian(data, (ushort)Mode);
        }

        /// <summary>
        /// Gets or sets the RH/T acceleration mode.
        /// </summary>
        public RhtAccelerationModes Mode { get; set; }

        /// <summary>
        /// By default, the RH/T acceleration algorithm is optimized for a sensor which is positioned in free air. If the sensor is integrated
        /// into another device, the ambient RH/T output values might not be optimal due to different thermal behavior. This parameter can be
        /// used to adapt the RH/T acceleration behavior for the actual use-case, leading in an improvement of the ambient RH/T output accuracy.
        /// </summary>
        public enum RhtAccelerationModes : ushort
        {
            /// <summary>
            /// Default / Air Purifier / IAQ (slow).
            /// </summary>
            Slow = 0,

            /// <summary>
            /// IAQ (fast).
            /// </summary>
            Fast = 1,

            /// <summary>
            /// IAQ (medium).
            /// </summary>
            Medium = 2
        }
    }
}
