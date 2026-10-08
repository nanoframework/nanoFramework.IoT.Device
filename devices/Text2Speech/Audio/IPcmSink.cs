// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Receives blocks of unsigned 8-bit mono PCM samples.
    /// </summary>
    public interface IPcmSink
    {
        /// <summary>
        /// Writes PCM samples to the destination.
        /// </summary>
        /// <param name="buffer">The unsigned 8-bit PCM samples.</param>
        /// <param name="offset">The zero-based offset of the first sample.</param>
        /// <param name="count">The number of samples to write.</param>
        void Write(byte[] buffer, int offset, int count);
    }
}
