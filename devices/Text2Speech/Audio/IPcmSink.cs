// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
