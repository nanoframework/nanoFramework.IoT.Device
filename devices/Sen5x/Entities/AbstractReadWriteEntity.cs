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
        internal virtual void ToSpanByte(SpanByte data) => throw new NotImplementedException();
    }
}
