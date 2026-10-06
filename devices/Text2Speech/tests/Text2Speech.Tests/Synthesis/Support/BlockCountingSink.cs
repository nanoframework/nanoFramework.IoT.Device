// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

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
