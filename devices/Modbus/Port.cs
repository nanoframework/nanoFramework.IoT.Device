// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using System.Threading;

namespace Iot.Device.Modbus
{
    /// <summary>
    /// Represents a serial port used for Modbus communication.
    /// </summary>
    public abstract class Port : IDisposable
    {
        private SerialPort _serialPort;

        /// <summary>
        /// Formats an array of bytes as a string.
        /// </summary>
        /// <param name="buffer">The buffer to format.</param>
        /// <returns>A string representation of the buffer in hexadecimal format.</returns>
        protected static string Format(byte[] buffer)
        {
            if (buffer.Length > 0)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var b in buffer)
                {
                    sb.Append(b.ToString("X2"));
                    sb.Append(' ');
                }

                return sb.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Port"/> class with the specified port settings.
        /// </summary>
        /// <param name="portName">The name of the serial port.</param>
        /// <param name="baudRate">The baud rate.</param>
        /// <param name="parity">The parity.</param>
        /// <param name="dataBits">The number of data bits.</param>
        /// <param name="stopBits">The number of stop bits.</param>
        /// <param name="receivedBytesThreshold">The number of bytes required before the DataReceived event is fired. Default is 1.</param>
        /// <param name="mode">The mode of serial port, default is <see cref="SerialMode.RS485"/>.</param>
        protected Port(
            string portName,
            int baudRate,
            Parity parity,
            int dataBits,
            StopBits stopBits,
            int receivedBytesThreshold = 1,
            SerialMode mode = SerialMode.RS485)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPort.Mode = mode;
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReceivedBytesThreshold = receivedBytesThreshold;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Port"/> class with the specified serial port.
        /// </summary>
        /// <param name="port">The serial port.</param>
        /// <param name="receivedBytesThreshold">The number of bytes required before the <see cref="DataReceived"/> event is fired. Default is 1.</param>
        /// <param name="mode">The mode of serial port, default is <see cref="SerialMode.RS485"/>.</param>
        protected Port(
            SerialPort port,
            int receivedBytesThreshold = 1,
            SerialMode mode = SerialMode.RS485)
        {
            _serialPort = port;
            _serialPort.Mode = mode;

            if (receivedBytesThreshold != 1)
            {
                _serialPort.ReceivedBytesThreshold = receivedBytesThreshold;
            }
        }

        /// <summary>
        /// Gets the name of the serial port.
        /// </summary>
        public string PortName => _serialPort != null ? _serialPort.PortName : string.Empty;

        /// <summary>
        /// Gets a value indicating whether the serial port is open.
        /// </summary>
        public bool IsOpen => _serialPort != null && _serialPort.IsOpen;

        /// <summary>
        /// Gets or sets the read timeout in milliseconds.
        /// </summary>
        public int ReadTimeout
        {
            get => _serialPort != null ? _serialPort.ReadTimeout : Timeout.Infinite;

            set
            {
                if (_serialPort != null)
                {
                    _serialPort.ReadTimeout = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the write timeout in milliseconds.
        /// </summary>
        public int WriteTimeout
        {
            get => _serialPort != null ? _serialPort.WriteTimeout : Timeout.Infinite;

            set
            {
                if (_serialPort != null)
                {
                    _serialPort.WriteTimeout = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the input buffer.
        /// </summary>
        public int ReadBufferSize
        {
            get => _serialPort != null ? _serialPort.ReadBufferSize : 0;

            set
            {
                if (_serialPort != null)
                {
                    _serialPort.ReadBufferSize = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the output buffer.
        /// </summary>
        public int WriteBufferSize
        {
            get => _serialPort != null ? _serialPort.WriteBufferSize : 0;

            set
            {
                if (_serialPort != null)
                {
                    _serialPort.WriteBufferSize = value;
                }
            }
        }

        /// <summary>
        /// Checks if the serial port is open and opens it if necessary.
        /// </summary>
        /// <returns><see langword="true"/> if the serial port is open, <see langword="false"/> otherwise.</returns>
        protected bool CheckOpen()
        {
            if (_serialPort != null)
            {
                if (!_serialPort.IsOpen)
                {
                    _serialPort.DataReceived += (sender, args) =>
                    {
                        if (_serialPort.BytesToRead > 0)
                        {
                            DataReceived(_serialPort.BytesToRead);
                        }
                    };

                    _serialPort.Open();
                }

                return _serialPort.IsOpen;
            }

            return false;
        }

        /// <summary>
        /// Event handler for the DataReceived event.
        /// </summary>
        /// <param name="bytesToRead">The number of bytes available to read.</param>
        protected virtual void DataReceived(int bytesToRead)
        {
        }

        /// <summary>
        /// Reads a single byte of data from the serial port.
        /// </summary>
        /// <returns>The byte read from the serial port.</returns>
        protected byte DataRead()
        {
            if (CheckOpen())
            {
                return (byte)_serialPort.ReadByte();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Reads a specified number of bytes from the serial port.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>An array of bytes read from the serial port.</returns>
        protected byte[] DataRead(int length)
        {
            var buffer = new byte[length];

            if (CheckOpen())
            {
                int offset = 0;

                while (offset < buffer.Length)
                {
                    int count = _serialPort.Read(buffer, offset, buffer.Length - offset);

                    if (count < 1)
                    {
                        break;
                    }

                    offset += count;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Writes a single byte of data to the serial port.
        /// </summary>
        /// <param name="value">The byte to write to the serial port.</param>
        protected void DataWrite(byte value)
        {
            if (CheckOpen())
            {
                _serialPort.WriteByte(value);
            }
        }

        /// <summary>
        /// Writes a specified number of bytes to the serial port.
        /// </summary>
        /// <param name="buffer">The buffer that contains the data to write.</param>
        /// <param name="offset">The zero-based byte offset in the buffer at which to begin writing.</param>
        /// <param name="count">The number of bytes to write.</param>
        protected void DataWrite(byte[] buffer, int offset, int count)
        {
            if (CheckOpen())
            {
                _serialPort.Write(buffer, offset, count);
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"{PortName} TX ({buffer.Length}): {Format(buffer)}");
#endif
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the serial port and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_serialPort != null)
                {
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }

                    _serialPort.Dispose();
                    _serialPort = null;
                }
            }
        }

        /// <summary>
        /// Releases the resources used by the serial port.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
