// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using System.Text;
using Iot.Device.Common.GnssDevice;

namespace Iot.Device.Atgm336h
{
    /// <summary>
    /// Class for controlling Atgm336h GPS module.
    /// </summary>
    public class Atgm336h : GnssDevice, IDisposable
    {
        /// <summary>
        /// Delegate for the error handler when parsing the GPS data.
        /// </summary>
        /// <param name="exception">The exception that occurred during parsing.</param>
        public delegate void ParsingErrorHandler(Exception exception);

        /// <summary>
        /// Event handler for parsing errors that occur during data processing of the ATGM336H GPS module.
        /// </summary>
        public event ParsingErrorHandler ParsingError;
        
        private readonly bool _shouldDispose;
        
        private SerialPort _serialDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Atgm336h" /> class.
        /// </summary>
        /// <param name="serialPort">Serial port name.</param>
        public Atgm336h(string serialPort)
        {
            _serialDevice = new SerialPort(serialPort);
            _serialDevice.BaudRate = 9600;
            _serialDevice.Parity = Parity.None;
            _serialDevice.StopBits = StopBits.One;
            _serialDevice.Handshake = Handshake.None;
            _serialDevice.DataBits = 8;
            _serialDevice.ReadBufferSize = 2048;
            _serialDevice.ReadTimeout = 2000;
            _serialDevice.WatchChar = '\r';
            _shouldDispose = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Atgm336h" /> class.
        /// </summary>
        /// <param name="serialPort">Serial port instance.</param>
        public Atgm336h(SerialPort serialPort)
        {
            _serialDevice = serialPort;
            _shouldDispose = false;
        }

        /// <summary>
        /// Starts the GPS module by opening the serial device connection.
        /// </summary>
        public void Start()
        {
            if (_serialDevice.IsOpen)
            {
                return;
            }
            
            _serialDevice.DataReceived += SerialDevice_DataReceived;
            _serialDevice.Open();
        }

        /// <summary>
        /// Stops the communication with the GPS module by closing the serial port.
        /// </summary>
        public void Stop()
        {
            if (!_serialDevice.IsOpen)
            {
                return;
            }
            
            _serialDevice.DataReceived -= SerialDevice_DataReceived;
            _serialDevice.Close();
        }
        
        private void SerialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialDevice = (SerialPort)sender;
            if (serialDevice.BytesToRead == 0)
            {
                return;
            }

            var buffer = new byte[serialDevice.BytesToRead];
            var bytesRead = serialDevice.Read(buffer, 0, buffer.Length);
            var stringBuffer = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var commands = stringBuffer.Split(serialDevice.WatchChar);
            foreach (var command in commands)
            {
                try
                {
                    var commandTrimmed = command.StartsWith("\n") ? command.Substring(1) : command;
                    if (commandTrimmed.StartsWith("$GNGSA"))
                    {
                        var data = (GngsaData)Nmea0183Parser.Parse(commandTrimmed);
                        Fix = data.Fix;
                        GnssOperation = data.Mode;
                    }

                    if (Fix != Fix.NoFix && commandTrimmed.StartsWith("$GNGLL"))
                    {
                        var data = (GpgllData)Nmea0183Parser.Parse(commandTrimmed);
                        Location = data.Location;
                    }
                }
                catch (Exception exception)
                {
                    ParsingError?.Invoke(exception);
                }
            }
        }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            if (_serialDevice is null)
            {
                return;
            }
            
            if (_shouldDispose)
            {
                _serialDevice.DataReceived -= SerialDevice_DataReceived;
                _serialDevice.Dispose();
                _serialDevice = null;
            }
        }
    }
}