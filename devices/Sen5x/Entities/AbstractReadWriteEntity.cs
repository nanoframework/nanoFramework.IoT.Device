// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// This abstract class is used as base class when a command both returns data but can also be written.
    /// </summary>
    public abstract class AbstractReadWriteEntity : AbstractReadEntity
    {
        /// <summary>
        /// The entity will write its data contents into the provided buffer, which already has the correct size according to <see cref="AbstractReadEntity.ByteCount"/>. The
        /// only commonality is that every 3rd byte is always a checksum byte and must be skipped. The checksum is calculated after this method is called, so it can be
        /// skipped by the implemented method.
        /// </summary>
        /// <param name="data">The data buffer that needs to be filled in by this entity.</param>
        internal abstract void ToSpanByte(Span<byte> data);
    }
}
