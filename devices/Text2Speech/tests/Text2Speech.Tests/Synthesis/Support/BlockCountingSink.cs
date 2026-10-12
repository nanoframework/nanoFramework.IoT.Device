// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Counts streamed PCM writes and their maximum size.
    /// </summary>
    internal sealed class BlockCountingSink : IPcmSink
    {
        internal int ByteCount { get; private set; }

        internal int MaximumBlockSize { get; private set; }

        internal int WriteCount { get; private set; }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            ByteCount += count;
            WriteCount++;
            if (count > MaximumBlockSize)
            {
                MaximumBlockSize = count;
            }
        }
    }
}
