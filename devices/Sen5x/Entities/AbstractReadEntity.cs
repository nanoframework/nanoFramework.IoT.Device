// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using UnitsNet;

namespace Iot.Device.Sen5x.Entities
{
    /// <summary>
    /// This abstract class is used as base class when a command returns data that needs to be represented.
    /// </summary>
    public abstract class AbstractReadEntity
    {
        internal virtual int ByteCount => throw new NotImplementedException();

        internal virtual void FromSpanByte(SpanByte data) => throw new NotImplementedException();
    }
}
