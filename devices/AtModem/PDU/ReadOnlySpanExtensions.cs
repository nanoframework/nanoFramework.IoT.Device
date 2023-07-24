// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem.PDU
{
    internal static class ReadOnlySpanExtensions
    {
        public static SpanChar SliceOnIndex(this SpanChar span, int start, int end)
        {
            return span.Slice(start, end - start);
        }
    }
}
