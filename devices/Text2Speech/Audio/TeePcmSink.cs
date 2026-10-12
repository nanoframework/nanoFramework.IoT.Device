// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Forwards each PCM block to two sinks in order.
    /// </summary>
    public sealed class TeePcmSink : IPcmSink
    {
        private readonly IPcmSink _first;
        private readonly IPcmSink _second;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeePcmSink" /> class.
        /// </summary>
        /// <param name="first">The first destination.</param>
        /// <param name="second">The second destination.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="first" /> or <paramref name="second" /> is <see langword="null" />.
        /// </exception>
        public TeePcmSink(IPcmSink first, IPcmSink second)
        {
            if (first == null || second == null)
            {
                throw new ArgumentNullException();
            }

            _first = first;
            _second = second;
        }

        /// <summary>
        /// Writes a PCM block to both destinations.
        /// </summary>
        /// <param name="buffer">The unsigned 8-bit PCM samples.</param>
        /// <param name="offset">The zero-based source offset.</param>
        /// <param name="count">The number of samples to write.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            _first.Write(buffer, offset, count);
            _second.Write(buffer, offset, count);
        }
    }
}
