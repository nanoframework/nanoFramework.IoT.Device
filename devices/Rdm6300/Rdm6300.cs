// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;

namespace Iot.Device.Rdm6300
{
    /// <summary>
    /// Represents an RDM6300 125 kHz RFID reader connected through UART.
    /// </summary>
    public class Rdm6300 : IDisposable
    {
        /// <summary>
        /// The fixed UART baud rate used by the RDM6300.
        /// </summary>
        public const int DefaultBaudRate = 9600;

        /// <summary>
        /// The number of bytes in an RDM6300 frame.
        /// </summary>
        public const int FrameLength = 14;

        private const byte StartOfText = 0x02;
        private const byte EndOfText = 0x03;

        private readonly SerialPort _serialPort;
        private readonly byte[] _frame = new byte[FrameLength];
        private int _framePosition;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rdm6300"/> class.
        /// </summary>
        /// <param name="portName">The serial port name, for example COM2.</param>
        public Rdm6300(string portName)
        {
            _serialPort = new SerialPort(portName)
            {
                BaudRate = DefaultBaudRate,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
            };

            _serialPort.DataReceived += SerialPortDataReceived;
            _serialPort.Open();
        }

        /// <summary>
        /// Occurs when a valid RFID tag is received.
        /// </summary>
        public event Rdm6300TagDetectedEventHandler TagDetected;

        /// <summary>
        /// Gets the most recently received tag identifier as ten hexadecimal characters.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Gets the version or customer code from the most recently received tag.
        /// </summary>
        public byte TagVersion { get; private set; }

        /// <summary>
        /// Gets the four-byte identifier from the most recently received tag.
        /// </summary>
        public uint TagId { get; private set; }

        /// <summary>
        /// Validates and parses a complete 14-byte RDM6300 frame.
        /// </summary>
        /// <param name="frame">The frame to parse.</param>
        /// <param name="tag">The parsed tag identifier when the frame is valid.</param>
        /// <returns><see langword="true"/> when the frame and its checksum are valid; otherwise, <see langword="false"/>.</returns>
        public static bool TryParseFrame(byte[] frame, out string tag)
        {
            return TryParseFrame(frame, out tag, out _, out _);
        }

        /// <summary>
        /// Validates and parses a complete 14-byte RDM6300 frame.
        /// </summary>
        /// <param name="frame">The frame to parse.</param>
        /// <param name="tag">The complete tag value as ten hexadecimal characters.</param>
        /// <param name="tagVersion">The parsed version or customer code.</param>
        /// <param name="tagId">The parsed four-byte tag identifier.</param>
        /// <returns><see langword="true"/> when the frame and its checksum are valid; otherwise, <see langword="false"/>.</returns>
        public static bool TryParseFrame(byte[] frame, out string tag, out byte tagVersion, out uint tagId)
        {
            tag = null;
            tagVersion = 0;
            tagId = 0;
            if (frame == null || frame.Length != FrameLength || frame[0] != StartOfText || frame[FrameLength - 1] != EndOfText)
            {
                return false;
            }

            byte checksum = 0;
            byte highNibble = 0;
            byte parsedTagVersion = 0;
            uint parsedTagId = 0;
            char[] tagCharacters = new char[10];
            for (int index = 0; index < tagCharacters.Length; index++)
            {
                if (!TryParseHexDigit(frame[index + 1], out byte digit))
                {
                    return false;
                }

                tagCharacters[index] = (char)frame[index + 1];
                if ((index & 1) == 1)
                {
                    checksum ^= (byte)((highNibble << 4) | digit);
                }
                else
                {
                    highNibble = digit;
                }

                if (index < 2)
                {
                    parsedTagVersion = (byte)((parsedTagVersion << 4) | digit);
                }
                else
                {
                    parsedTagId = (parsedTagId << 4) | digit;
                }
            }

            if (!TryParseHexDigit(frame[11], out byte checksumHigh) || !TryParseHexDigit(frame[12], out byte checksumLow))
            {
                return false;
            }

            if (checksum != (byte)((checksumHigh << 4) | checksumLow))
            {
                return false;
            }

            tag = new string(tagCharacters);
            tagVersion = parsedTagVersion;
            tagId = parsedTagId;
            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _serialPort.DataReceived -= SerialPortDataReceived;
            _serialPort.Dispose();
            _disposed = true;
        }

        private static bool TryParseHexDigit(byte value, out byte digit)
        {
            if (value >= '0' && value <= '9')
            {
                digit = (byte)(value - '0');
                return true;
            }

            if (value >= 'A' && value <= 'F')
            {
                digit = (byte)(value - 'A' + 10);
                return true;
            }

            if (value >= 'a' && value <= 'f')
            {
                digit = (byte)(value - 'a' + 10);
                return true;
            }

            digit = 0;
            return false;
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (_serialPort.BytesToRead > 0)
            {
                ProcessByte((byte)_serialPort.ReadByte());
            }
        }

        private void ProcessByte(byte value)
        {
            if (value == StartOfText)
            {
                _framePosition = 0;
            }
            else if (_framePosition == 0)
            {
                return;
            }

            _frame[_framePosition++] = value;
            if (_framePosition != FrameLength)
            {
                return;
            }

            _framePosition = 0;
            string tag;
            if (TryParseFrame(_frame, out tag, out byte tagVersion, out uint tagId))
            {
                Tag = tag;
                TagVersion = tagVersion;
                TagId = tagId;
                TagDetected?.Invoke(this, tag);
            }
        }
    }

    /// <summary>
    /// Represents the method that handles a detected RDM6300 tag.
    /// </summary>
    /// <param name="sender">The RDM6300 instance that detected the tag.</param>
    /// <param name="tag">The tag identifier as ten hexadecimal characters.</param>
    public delegate void Rdm6300TagDetectedEventHandler(object sender, string tag);
}
