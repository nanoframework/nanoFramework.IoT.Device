// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Collects streamed PCM blocks into a preallocated byte array.
    /// </summary>
    internal sealed class ByteArrayPcmSink : IPcmSink
    {
        private readonly byte[] _buffer;
        private int _position;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayPcmSink" /> class.
        /// </summary>
        /// <param name="length">The exact number of PCM bytes to collect.</param>
        public ByteArrayPcmSink(int length)
        {
            _buffer = new byte[length];
        }

        /// <summary>
        /// Gets the collected PCM buffer.
        /// </summary>
        /// <value>The preallocated PCM destination buffer.</value>
        public byte[] Buffer => _buffer;

        /// <summary>
        /// Copies a PCM block into the destination buffer.
        /// </summary>
        /// <param name="buffer">The source PCM buffer.</param>
        /// <param name="offset">The zero-based source offset.</param>
        /// <param name="count">The number of bytes to copy.</param>
        /// <exception cref="InvalidOperationException">The block exceeds the calculated output size.</exception>
        public void Write(byte[] buffer, int offset, int count)
        {
            if (_position + count > _buffer.Length)
            {
                throw new InvalidOperationException();
            }

            Array.Copy(buffer, offset, _buffer, _position, count);
            _position += count;
        }
    }
}
