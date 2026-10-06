// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Iot.Device.Text2Speech.Samples
{
    /// <summary>
    /// Streams PCM into a WAV file and finalizes its header when synthesis completes.
    /// </summary>
    internal sealed class WavFileWriter : IPcmSink, IDisposable
    {
        private readonly FileStream _stream;
        private readonly string _path;
        private int _dataLength;
        private bool _completed;
        private bool _aborted;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="WavFileWriter" /> class for the specified file.
        /// </summary>
        /// <param name="path">The destination WAV file path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is empty.</exception>
        public WavFileWriter(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException();
            }

            if (path.Length == 0)
            {
                throw new ArgumentException();
            }

            _path = path;
            _stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            byte[] placeholder = new byte[WavPcm8Mono8000.HeaderSize];
            _stream.Write(placeholder, 0, placeholder.Length);
        }

        /// <summary>
        /// Writes a block of unsigned 8-bit PCM samples.
        /// </summary>
        /// <param name="buffer">The PCM source buffer.</param>
        /// <param name="offset">The zero-based source offset.</param>
        /// <param name="count">The number of PCM bytes to write.</param>
        /// <exception cref="InvalidOperationException">The writer is completed, aborted, or disposed.</exception>
        public void Write(byte[] buffer, int offset, int count)
        {
            if (_completed || _aborted || _disposed)
            {
                throw new InvalidOperationException();
            }

            _stream.Write(buffer, offset, count);
            _dataLength += count;
        }

        /// <summary>
        /// Writes the final WAV header and flushes the file.
        /// </summary>
        /// <exception cref="InvalidOperationException">The writer is aborted or disposed.</exception>
        public void Complete()
        {
            if (_completed)
            {
                return;
            }

            if (_aborted || _disposed)
            {
                throw new InvalidOperationException();
            }

            _stream.Seek(0, SeekOrigin.Begin);
            byte[] header = WavPcm8Mono8000.CreateHeader(_dataLength);
            _stream.Write(header, 0, header.Length);
            _stream.Flush();
            _completed = true;
        }

        /// <summary>
        /// Closes and removes an incomplete WAV file after synthesis fails.
        /// </summary>
        public void Abort()
        {
            if (_disposed)
            {
                return;
            }

            _aborted = true;
            _stream.Dispose();
            _disposed = true;
            File.Delete(_path);
        }

        /// <summary>
        /// Finalizes and closes the WAV file.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                if (!_aborted)
                {
                    Complete();
                }
            }
            finally
            {
                _stream.Dispose();
                _disposed = true;
            }
        }
    }
}
