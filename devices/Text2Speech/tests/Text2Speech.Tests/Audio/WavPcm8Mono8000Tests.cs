// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using nanoFramework.TestFramework;

namespace Iot.Device.Text2Speech.Tests
{
    /// <summary>
    /// Contains regression tests for <see cref="WavPcm8Mono8000"/>.
    /// </summary>
    [TestClass]
    public class WavPcm8Mono8000Tests
    {
        /// <summary>
        /// Verifies the canonical compact WAV header.
        /// </summary>
        [TestMethod]
        public void HeaderContainsExpectedPcmFormat()
        {
            const int DataLength = 12345;
            byte[] header = WavPcm8Mono8000.CreateHeader(DataLength);

            Assert.AreEqual(44, header.Length, "Header length");
            Assert.AreEqual((byte)'R', header[0], "RIFF marker");
            Assert.AreEqual((byte)'W', header[8], "WAVE marker");
            Assert.AreEqual(8000, ReadInt32(header, 24), "Sample rate");
            Assert.AreEqual(8000, ReadInt32(header, 28), "Byte rate");
            Assert.AreEqual(1, ReadInt16(header, 22), "Channels");
            Assert.AreEqual(1, ReadInt16(header, 32), "Block alignment");
            Assert.AreEqual(8, ReadInt16(header, 34), "Bits per sample");
            Assert.AreEqual(DataLength, ReadInt32(header, 40), "Data length");
            Assert.AreEqual(
                DataLength,
                WavPcm8Mono8000.ValidateHeader(header, DataLength),
                "Validated length");
        }

        /// <summary>
        /// Verifies incompatible and truncated WAV data is rejected.
        /// </summary>
        [TestMethod]
        public void InvalidWavIsRejected()
        {
            byte[] stereo = WavPcm8Mono8000.CreateHeader(100);
            stereo[22] = 2;
            Assert.ThrowsException(
                typeof(ArgumentException),
                delegate { WavPcm8Mono8000.ValidateHeader(stereo, 100); },
                "Stereo WAV");

            byte[] truncated = WavPcm8Mono8000.CreateHeader(100);
            Assert.ThrowsException(
                typeof(ArgumentException),
                delegate { WavPcm8Mono8000.ValidateHeader(truncated, 99); },
                "Truncated data");
        }

        private static int ReadInt16(byte[] value, int offset)
        {
            return value[offset] | (value[offset + 1] << 8);
        }

        private static int ReadInt32(byte[] value, int offset)
        {
            return value[offset]
                | (value[offset + 1] << 8)
                | (value[offset + 2] << 16)
                | (value[offset + 3] << 24);
        }
    }
}
