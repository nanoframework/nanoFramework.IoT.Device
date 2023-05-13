// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// This abstract class is used as base class when a command returns data that needs to be represented.
    /// </summary>
    public abstract class AbstractReadEntity
    {
        /// <summary>
        /// Gets the number of bytes that is needed for this entity. This number includes checksum bytes.
        /// </summary>
        internal abstract int ByteCount { get; }

        /// <summary>
        /// Parses a received buffer of data and fills the fields of this entity. The only commonality in the format is that every 3rd byte is always a checksum byte and must
        /// be skipped. The checksum is already verified when this method is called and therefor not needed anymore.
        /// </summary>
        /// <param name="data">The data received from the I2C exchange.</param>
        internal abstract void FromSpanByte(SpanByte data);
    }
}
