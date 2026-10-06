// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Compares streamed PCM with a reference buffer.
    /// </summary>
    internal sealed class ComparingSink : IPcmSink
    {
        private readonly byte[] _expected;

        internal ComparingSink(byte[] expected)
        {
            _expected = expected;
            Matches = true;
        }

        internal int Position { get; private set; }

        internal bool Matches { get; private set; }

        /// <inheritdoc />
        public void Write(byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (Position >= _expected.Length || buffer[offset + i] != _expected[Position])
                {
                    Matches = false;
                }

                Position++;
            }
        }
    }
}
