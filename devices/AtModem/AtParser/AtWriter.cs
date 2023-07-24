// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents an AT command writer that write AT command to a stream.
    /// </summary>
    public class AtWriter : IAtWriter, IDisposable
    {
        private SerialPort _writer;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtWriter"/> class with the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        public AtWriter(SerialPort stream)
        {
            _writer = stream;
        }

        /// <inheritdoc/>
        public void Write(byte[] content)
        {
            _writer.Write(content, 0, content.Length);
        }

        /// <inheritdoc/>
        public void WriteLineAsync(string command, CancellationToken cancellationToken = default)
        {
            WriteAsync(command, cancellationToken);
            WriteAsync("\r", cancellationToken);
        }

        /// <inheritdoc/>
        public void WriteSmsPduAndCtrlZAsync(string smsPdu, CancellationToken cancellationToken = default)
        {
            WriteAsync(smsPdu, cancellationToken);
            WriteAsync("\x1A", cancellationToken);
        }

        /// <summary>
        /// Writes the specified text asynchronously to the underlying stream.
        /// </summary>
        /// <param name="text">The text to write.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        protected void WriteAsync(string text, CancellationToken cancellationToken = default)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            _writer.Write(buffer, 0, buffer.Length);
        }

        /// <inheritdoc/>
        public void Close()
        {
            Dispose();
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _writer.Dispose();
                    _writer = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _isDisposed = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
