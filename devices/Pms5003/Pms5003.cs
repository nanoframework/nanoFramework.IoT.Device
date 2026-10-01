// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Model;
using System.IO.Ports;
using Iot.Device.Pmsx003.Shared;

namespace Iot.Device.Pms5003
{
    /// <summary>
    /// Plantower PMS5003 particulate-matter sensor using its UART interface.
    /// </summary>
    public sealed class Pms5003 : IDisposable
    {
        /// <summary>The UART baud rate required by the sensor.</summary>
        public const int DefaultBaudRate = 9600;

        private readonly bool _shouldDispose;
        private SerialPort _serialPort;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pms5003"/> class.
        /// </summary>
        /// <param name="serialPort">The serial port configured for 9600 baud, 8 data bits, no parity, and one stop bit.</param>
        /// <param name="shouldDispose">True to dispose the serial port when this instance is disposed.</param>
        public Pms5003(SerialPort serialPort, bool shouldDispose = true)
        {
            _serialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Reads and validates the next complete sensor measurement.
        /// </summary>
        /// <returns>The particulate-matter measurement.</returns>
        [Telemetry]
        public PmsReading Read()
        {
            if (_serialPort == null)
            {
                throw new ObjectDisposedException(nameof(Pms5003));
            }

            byte[] frame = new byte[Pmsx003Parser.FrameLength];
            SynchronizeHeader(frame);
            ReadExactly(frame, 2, frame.Length - 2);
            return new PmsReading(Pmsx003Parser.Parse(frame));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose && _serialPort != null)
            {
                _serialPort.Dispose();
            }

            _serialPort = null;
        }

        private void SynchronizeHeader(byte[] frame)
        {
            byte[] current = new byte[1];
            bool headerStarted = false;

            while (true)
            {
                ReadExactly(current, 0, 1);
                if (!headerStarted)
                {
                    headerStarted = current[0] == 0x42;
                    continue;
                }

                if (current[0] == 0x4D)
                {
                    frame[0] = 0x42;
                    frame[1] = 0x4D;
                    return;
                }

                headerStarted = current[0] == 0x42;
            }
        }

        private void ReadExactly(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int bytesRead = _serialPort.Read(buffer, offset, count);
                if (bytesRead <= 0)
                {
                    continue;
                }

                offset += bytesRead;
                count -= bytesRead;
            }
        }
    }
}