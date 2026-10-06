// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Iot.Device.Text2Speech
{
    /// <summary>
    /// Creates and validates canonical WAV headers for unsigned 8-bit mono PCM at 8 kHz.
    /// </summary>
    public static class WavPcm8Mono8000
    {
        /// <summary>
        /// The size of the canonical RIFF/WAVE header.
        /// </summary>
        public const int HeaderSize = 44;

        /// <summary>
        /// Creates a canonical WAV header for a PCM data block.
        /// </summary>
        /// <param name="dataLength">The number of PCM bytes following the header.</param>
        /// <returns>A 44-byte RIFF/WAVE header.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="dataLength" /> is negative.
        /// </exception>
        public static byte[] CreateHeader(int dataLength)
        {
            if (dataLength < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] header = new byte[HeaderSize];
            WriteAscii(header, 0, "RIFF");
            WriteUInt32(header, 4, dataLength + 36);
            WriteAscii(header, 8, "WAVE");
            WriteAscii(header, 12, "fmt ");
            WriteUInt32(header, 16, 16);
            WriteUInt16(header, 20, 1);
            WriteUInt16(header, 22, TtsSynthesizer.Channels);
            WriteUInt32(header, 24, TtsSynthesizer.SampleRate);
            WriteUInt32(header, 28, TtsSynthesizer.SampleRate);
            WriteUInt16(header, 32, 1);
            WriteUInt16(header, 34, TtsSynthesizer.BitsPerSample);
            WriteAscii(header, 36, "data");
            WriteUInt32(header, 40, dataLength);
            return header;
        }

        /// <summary>
        /// Validates a canonical Text2Speech WAV header and returns its PCM data length.
        /// </summary>
        /// <param name="header">A buffer containing at least the first 44 WAV bytes.</param>
        /// <param name="availableDataLength">The number of bytes available after the header.</param>
        /// <returns>The PCM data length declared by the WAV header.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="header" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="header" /> is shorter than 44 bytes, does not contain a canonical RIFF/WAVE
        /// PCM header, does not describe unsigned 8-bit mono PCM at 8 kHz, or declares a data length
        /// outside <paramref name="availableDataLength" />.
        /// </exception>
        public static int ValidateHeader(byte[] header, long availableDataLength)
        {
            if (header == null)
            {
                throw new ArgumentNullException();
            }

            if (header.Length < HeaderSize)
            {
                throw new ArgumentException();
            }

            if (!Matches(header, 0, "RIFF")
                || !Matches(header, 8, "WAVE")
                || !Matches(header, 12, "fmt ")
                || !Matches(header, 36, "data"))
            {
                throw new ArgumentException();
            }

            if (ReadUInt16(header, 20) != 1
                || ReadUInt16(header, 22) != TtsSynthesizer.Channels
                || ReadUInt32(header, 24) != TtsSynthesizer.SampleRate
                || ReadUInt32(header, 28) != TtsSynthesizer.SampleRate
                || ReadUInt16(header, 32) != 1
                || ReadUInt16(header, 34) != TtsSynthesizer.BitsPerSample)
            {
                throw new ArgumentException();
            }

            int length = ReadUInt32(header, 40);
            if (length < 0 || length > availableDataLength)
            {
                throw new ArgumentException();
            }

            return length;
        }

        private static bool Matches(byte[] value, int offset, string expected)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                if (value[offset + i] != (byte)expected[i])
                {
                    return false;
                }
            }

            return true;
        }

        private static int ReadUInt16(byte[] value, int offset)
        {
            return value[offset] | (value[offset + 1] << 8);
        }

        private static int ReadUInt32(byte[] value, int offset)
        {
            return value[offset]
                | (value[offset + 1] << 8)
                | (value[offset + 2] << 16)
                | (value[offset + 3] << 24);
        }

        private static void WriteAscii(byte[] buffer, int offset, string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                buffer[offset + i] = (byte)value[i];
            }
        }

        private static void WriteUInt16(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        private static void WriteUInt32(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }
    }
}
