// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Iot.Device.Text2Speech.Samples
{
    /// <summary>
    /// Provides file-oriented helpers for the compact Text2Speech WAV format.
    /// </summary>
    internal static class WavFile
    {
        /// <summary>
        /// Writes a complete PCM buffer as a canonical Text2Speech WAV file.
        /// </summary>
        /// <param name="path">The destination file path.</param>
        /// <param name="pcm">The unsigned 8-bit PCM samples.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pcm" /> is <see langword="null" />.</exception>
        public static void Write(string path, byte[] pcm)
        {
            if (pcm == null)
            {
                throw new ArgumentNullException();
            }

            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                byte[] header = WavPcm8Mono8000.CreateHeader(pcm.Length);
                stream.Write(header, 0, header.Length);
                stream.Write(pcm, 0, pcm.Length);
                stream.Flush();
            }
        }

        /// <summary>
        /// Validates a WAV stream and leaves it positioned at the first PCM byte.
        /// </summary>
        /// <param name="stream">The readable WAV stream.</param>
        /// <returns>The validated PCM data length.</returns>
        /// <exception cref="IOException">The stream ends before the complete WAV header is read.</exception>
        /// <exception cref="ArgumentException">The stream does not contain a supported WAV header.</exception>
        public static int ValidateAndPositionAtData(Stream stream)
        {
            byte[] header = new byte[WavPcm8Mono8000.HeaderSize];
            ReadExactly(stream, header, 0, header.Length);
            return WavPcm8Mono8000.ValidateHeader(
                header,
                stream.Length - WavPcm8Mono8000.HeaderSize);
        }

        private static void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read == 0)
                {
                    throw new IOException();
                }

                offset += read;
                count -= read;
            }
        }
    }
}
