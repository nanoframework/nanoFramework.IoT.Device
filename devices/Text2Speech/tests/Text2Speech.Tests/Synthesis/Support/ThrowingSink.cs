// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Interrupts a render when its first streaming block is delivered.
    /// </summary>
    internal sealed class ThrowingSink : IPcmSink
    {
        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Test sink interrupted rendering.");
        }
    }
}
