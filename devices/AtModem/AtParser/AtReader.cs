// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using IoT.Device.AtModem.CodingSchemes;

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents an AT command reader that reads AT command responses from a stream.
    /// </summary>
    public class AtReader : IAtReader, IDisposable
    {
        private static readonly byte[] EolSequence = new byte[] { (byte)'\r', (byte)'\n' };
        private static readonly byte[] SmsPromptSequence = new byte[] { (byte)'>', (byte)' ' };
        private readonly ArrayList _lineRead = new ArrayList();

        private SerialPort _reader;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtReader"/> class with the specified input stream.
        /// </summary>
        /// <param name="inputReader">The input stream to read from.</param>
        public AtReader(SerialPort inputReader)
        {
            _reader = inputReader;
        }

        /// <inheritdoc/>
        public void Open()
        {
            // TODO: can be simplified I guess
        }

        /// <inheritdoc/>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the current number of items available in the reader.
        /// </summary>
        /// <returns>The number of available items.</returns>
        public int AvailableItems()
        {
            return _reader.BytesToRead;
        }

        /// <inheritdoc/>
        public string Read(CancellationToken cancellationToken = default)
        {
            return ReadLine(cancellationToken);
        }

        /// <inheritdoc/>
        public byte[] ReadBytes(int count)
        {
            byte[] buffer = new byte[count];
            _reader.Read(buffer, 0, count);
            return buffer;
        }

        /// <inheritdoc/>
        public string ReadSingleLine(CancellationToken cancellationToken = default)
        {
            bool bloop = true;
            byte[] buff = new byte[1];
            byte prevbyte = 0;

            // Start clean
            _lineRead.Clear();

            while ((!cancellationToken.IsCancellationRequested) && bloop)
            {
                try
                {
                    if (_reader.BytesToRead > 0)
                    {
                        buff[0] = (byte)_reader.ReadByte();
                        
                        _lineRead.Add(buff[0]);

                        // do we have a new line?
                        if ((prevbyte == EolSequence[0]) && (buff[0] == EolSequence[1]))
                        {
                            // Let's remove the 2 last elements
                            _lineRead.RemoveAt(_lineRead.Count - 1);
                            _lineRead.RemoveAt(_lineRead.Count - 1);
                            bloop = false;
                        }

                        prevbyte = buff[0];
                    }
                }
                catch (Exception)
                {
                    // Nothing to read!
                    break;
                }
            }

            return ConvertHelper.ConvertAsciiToString(_lineRead);
        }

        private string ReadLine(CancellationToken cancellationToken = default)
        {
            string ret = string.Empty;
            try
            {
                // so do a loop and either finish to read all or return nothing
                _lineRead.Clear();
                bool bloop = true;
                byte[] buff = new byte[1];
                byte prevbyte = 0;

                while ((!cancellationToken.IsCancellationRequested) && bloop)
                {
                    try
                    {
                        if (_reader.BytesToRead > 0)
                        {
                            buff[0] = (byte)_reader.ReadByte();

                            _lineRead.Add(buff[0]);

                            // do we have a new line?
                            if ((prevbyte == EolSequence[0]) && (buff[0] == EolSequence[1]))
                            {
                                _lineRead.RemoveAt(_lineRead.Count - 1);
                                _lineRead.RemoveAt(_lineRead.Count - 1);
                                bloop = false;
                            }
                            else if ((prevbyte == SmsPromptSequence[0]) && (buff[0] == SmsPromptSequence[1]))
                            {
                                // we have a prompt for SMS
                                bloop = false;
                            }

                            prevbyte = buff[0];
                        }
                    }
                    catch (Exception)
                    {
                        // Nothing to read!
                        break;
                    }
                }

                ret = ConvertHelper.ConvertAsciiToString(_lineRead);
            }
            catch (Exception ecx)
            {
                Debug.WriteLine("Cannot parse : " + ecx.StackTrace);
            }

            return ret;
        }

        #region Dispose

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _reader = null;
                }

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

        #endregion
    }
}
