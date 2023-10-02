//
// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.
//

using System.IO;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http
{
    /// <summary>
    /// A base class representing an HTTP entity body and content headers.
    /// </summary>
    public abstract class HttpContent : IDisposable
    {
        private HttpContentHeaders _headers;
        private FixedMemoryStream _buffer;
        private Stream _stream;

        private bool _disposed;

        internal const int MaxBufferSize = int.MaxValue;
        internal static readonly Encoding DefaultStringEncoding = Encoding.UTF8;

        /// <summary>
        /// Gets the HTTP content headers as defined in RFC 2616.
        /// </summary>
        /// <value>The content headers as defined in RFC 2616.</value>
        public HttpContentHeaders Headers
        {
            get
            {
                if (_headers == null)
                {
                    _headers = new HttpContentHeaders(this);
                }

                return _headers;
            }
        }

        /// <summary>
        /// Contains the actual bytes read into the buffer
        /// </summary>
        protected int TotalBytesRead { get; set; }

        /// <summary>
        /// Initializes a new instance of the HttpContent class.
        /// </summary>
        protected HttpContent()
        {
        }

        /// <summary>
        /// Serializes the HTTP content into a stream of bytes and copies it to <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="stream"/> was <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>
        /// This is the .NET nanoFramework equivalent of HttpContent.CopyTo(Stream, TransportContext, CancellationToken).
        /// </para>
        /// </remarks>
        public void CopyTo(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException();
            }

            if (_buffer != null)
            {
                _buffer.CopyTo(stream);
            }

            SerializeToStream(stream);
        }

        /// <summary>
        /// Serialize the HTTP content to a memory buffer.
        /// </summary>
        /// <returns>The Stream with the HTTP content.</returns>
        /// <remarks>
        /// <para>
        /// This operation will block.
        /// </para>
        /// <para>
        /// After content is serialized to a memory buffer, calls to one of the CopyTo methods will copy the content of the memory buffer to the target stream.
        /// </para>
        /// <para>
        /// This is the .NET nanoFramework equivalent of LoadIntoBufferAsync.
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">If the object has been disposed.</exception>
        public Stream LoadIntoBuffer()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(string.Empty);
            }

            if (_buffer != null)
            {
                return _buffer;
            }

            _buffer = new FixedMemoryStream(int.MaxValue);

            SerializeToStream(_buffer);

            if (TotalBytesRead > 0)
            {
                _buffer.SetLength(TotalBytesRead);
            }

            _buffer.Seek(0, SeekOrigin.Begin);

            return _buffer;
        }

        /// <summary>
        /// Serializes the HTTP content and returns a stream that represents the content.
        /// </summary>
        /// <returns>The stream that represents the HTTP content.</returns>
        /// <exception cref="ObjectDisposedException">If the object has been disposed.</exception>
        public Stream ReadAsStream()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(string.Empty);
            }

            if (_buffer != null)
            {

                return new MemoryStream(_buffer.GetBuffer());
            }

            if (_stream == null)
            {

                _stream = LoadIntoBuffer();
            }

            return _stream;
        }

        /// <summary>
        /// Serialize the HTTP content to a byte array as an synchronous operation.
        /// </summary>
        /// <returns>A byte array with the HTTP content.</returns>
        /// <remarks>
        /// <para>
        /// This operation will block.
        /// </para>
        /// <para>
        /// This is the .NET nanoFramework equivalent of ReadAsByteArrayAsync.
        /// </para>
        /// </remarks>
        public byte[] ReadAsByteArray()
        {
            LoadIntoBuffer();

            return _buffer.ToArray();
        }

        /// <summary>
        /// Serialize the HTTP content to a string as an synchronous operation.
        /// </summary>
        /// <returns>A string with the HTTP content.</returns>
        /// <remarks>
        /// <para>
        /// This operation will block.
        /// </para>
        /// <para>
        /// This supports only <see cref="Encoding.UTF8"/> encoding.
        /// </para>
        /// <para>
        /// This is the .NET nanoFramework equivalent of ReadAsStringAsync.
        /// </para>
        /// </remarks>
        public string ReadAsString()
        {
            LoadIntoBuffer();

            if (_buffer.Length == 0)
            {
                return string.Empty;
            }

            // we only support UTF-8
            return Encoding.UTF8.GetString(
                _buffer.GetBuffer(),
                0,
                (int)_buffer.Length);
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the HTTP content.</param>
        /// <returns><see langword="true"/> if <paramref name="length"/> is a valid length; otherwise, <see langword="false"/>.</returns>
        protected internal abstract bool TryComputeLength(out long length);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _disposed = true;

                if (_buffer != null)
                {
                    _buffer.Dispose();
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, serializes the HTTP content to a stream. Otherwise, throws a <see cref="NotSupportedException"/>.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">The method is not overridden in the derived class.</exception>
        /// <remarks>
        /// <para>
        /// This is the .NET nanoFramework equivalent of SerializeToStream(Stream, TransportContext, CancellationToken).
        /// </para>
        /// </remarks>
        protected abstract void SerializeToStream(Stream stream);

        private sealed class FixedMemoryStream : MemoryStream
        {
            readonly long _maxSize;

            public FixedMemoryStream(long maxSize)
                : base()
            {
                _maxSize = maxSize;
            }

            private void CheckOverflow(int count)
            {
                if (Length + count > _maxSize)
                {
                    throw new HttpRequestException();
                }
            }

            public override void WriteByte(byte value)
            {
                CheckOverflow(1);
                base.WriteByte(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                CheckOverflow(count);
                base.Write(buffer, offset, count);
            }
        }
    }
}
