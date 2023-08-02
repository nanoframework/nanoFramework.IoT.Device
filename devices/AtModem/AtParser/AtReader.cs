// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents an AT command reader that reads AT command responses from a stream.
    /// </summary>
    public class AtReader : IAtReader, IDisposable
    {
        private static readonly byte[] EolSequence = new byte[] { (byte)'\r', (byte)'\n' };
        private static readonly byte[] SmsPromptSequence = new byte[] { (byte)'>', (byte)' ' };

        private SerialPort _reader;
        private StringBuilder _lineRead = new StringBuilder();
        private bool _isDisposed;

        private static string GetUTF8StringFrombytes(byte[] byteVal)
        {
            byte[] btOne = new byte[1];
            StringBuilder sb = new StringBuilder();
            char uniChar;
            for (int i = 0; i < byteVal.Length; i++)
            {
                btOne[0] = byteVal[i];
                if (btOne[0] > 127)
                {
                    uniChar = Convert.ToChar(btOne[0]);
                    sb.Append(uniChar);
                }
                else
                {
                    sb.Append(new string(Encoding.UTF8.GetChars(btOne)));
                }
            }

            return sb.ToString();
        }

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
            return (int)_reader.BytesToRead;
        }

        /// <inheritdoc/>
        public string ReadAsync(CancellationToken cancellationToken = default)
        {
            return ReadLine(cancellationToken);
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

                            // TODO: so far only ASCII is used here but this can be improved for a proper UTF8 conversion as well
                            _lineRead.Append(GetUTF8StringFrombytes(buff));

                            // do we have a new line?
                            if ((prevbyte == EolSequence[0]) && (buff[0] == EolSequence[1]))
                            {
                                _lineRead.Remove(_lineRead.Length - 2, 2);
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

                ret = _lineRead.ToString();
                if (ret == null)
                {
                    ret = string.Empty;
                }
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
