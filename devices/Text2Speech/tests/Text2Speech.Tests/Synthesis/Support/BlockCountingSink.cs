// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Counts streamed PCM writes and their maximum size.
    /// </summary>
    internal sealed class BlockCountingSink : IPcmSink
    {
        private byte[] _firstBuffer;

        internal int ByteCount { get; private set; }

        internal int MaximumBlockSize { get; private set; }

        internal int WriteCount { get; private set; }

        internal bool ChangedBuffer { get; private set; }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            if (_firstBuffer == null)
            {
                _firstBuffer = buffer;
            }
            else if (_firstBuffer != buffer)
            {
                ChangedBuffer = true;
            }

            ByteCount += count;
            WriteCount++;
            if (count > MaximumBlockSize)
            {
                MaximumBlockSize = count;
            }
        }
    }
}
