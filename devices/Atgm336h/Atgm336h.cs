// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using System.Text;

namespace Iot.Device.Atgm336h
{
    /// <summary>
    /// Class for controlling Atgm336h GPS module.
    /// </summary>
    public class Atgm336h : IDisposable
    {
        /// <summary>
        /// Delegate type to handle the event when the GPS module fix status changes.
        /// </summary>
        /// <param name="fix">The new fix status.</param>
        public delegate void FixChangedHandler(Fix fix);

        /// <summary>
        /// Delegate type to handle the event when the GPS module mode changes.
        /// </summary>
        /// <param name="mode">The new GPS module mode.</param>
        public delegate void ModeChangedHandler(Mode mode);

        /// <summary>
        /// Delegate type to handle the event when the GPS module location changes.
        /// </summary>
        /// <param name="latitude">The new latitude.</param>
        /// <param name="longitude">The new longitude.</param>
        public delegate void LocationChangeHandler(double latitude, double longitude);
        
        /// <summary>
        /// Gets the fix status of the GPS module.
        /// </summary>
        public Fix Fix { get; private set; } = Fix.NoFix;

        /// <summary>
        /// Gets the mode of the GPS module.
        /// </summary>
        /// <value>
        /// The mode of the GPS module.
        /// </value>
        public Mode Mode { get; private set; } = Mode.Unknown;

        /// <summary>
        /// Gets the latitude value.
        /// </summary>
        public double Latitude { get; private set; }

        /// <summary>
        /// Gets the longitude value.
        /// </summary>
        public double Longitude { get; private set; }

        /// <summary>
        /// Represents the event handler for when the fix status of the GPS module changes.
        /// </summary>
        public event FixChangedHandler FixChanged;

        /// <summary>
        /// Event that occurs when the location changes.
        /// </summary>
        public event LocationChangeHandler LocationChanged;

        /// <summary>
        /// Represents the event that is raised when the mode of the GPS module is changed.
        /// </summary>
        public event ModeChangedHandler ModeChanged;
        
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
            _serialDevice.DataReceived += SerialDevice_DataReceived;
            _shouldDispose = true;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Atgm336h" /> class.
        /// </summary>
        /// <param name="serialPort">Serial port instance.</param>
        public Atgm336h(SerialPort serialPort)
        {
            _serialDevice = serialPort;
            _serialDevice.DataReceived += SerialDevice_DataReceived;
            _shouldDispose = false;
        }

        /// <summary>
        /// Starts the GPS module by opening the serial device connection.
        /// </summary>
        public void Start()
        {
            _serialDevice.Open();
        }

        /// <summary>
        /// Stops the communication with the GPS module by closing the serial port.
        /// </summary>
        public void Stop()
        {
            _serialDevice.Close();
        }
        
        private static double ConvertToGeoLocation(string data, string direction)
        {
            var degreesLength = data.Length > 10 ? 3 : 2;

            var degrees = double.Parse(data.Substring(0, degreesLength));
            var minutes = double.Parse(data.Substring(degreesLength));

            var result = degrees + (minutes / 60);

            if (direction == "S" || direction == "W")
            {
                return -result;
            }

            return result;
        }

        private static Mode ConvertToMode(string data)
        {
            switch (data)
            {
                case "M":
                    return Mode.Manual;
                case "A":
                    return Mode.Auto;
            }

            throw new Exception();
        }

        private static Fix ConvertToFix(string data)
        {
            switch (data)
            {
                case "1":
                    return Fix.NoFix;
                case "2":
                    return Fix.Fix2D;
                case "3":
                    return Fix.Fix3D;
            }

            throw new Exception();
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
                var commandTrimmed = command.StartsWith("\n") ? command.Substring(1) : command;
                if (commandTrimmed.StartsWith("$GNGSA"))
                {
                    var data = commandTrimmed.Split(',');
                    var mode = ConvertToMode(data[1]);
                    var fix = ConvertToFix(data[2]);

                    if (fix != Fix)
                    {
                        Fix = fix;
                        FixChanged?.Invoke(fix);
                    }

                    if (mode != Mode)
                    {
                        Mode = mode;
                        ModeChanged?.Invoke(mode);
                    }
                }

                if (Fix != Fix.NoFix && commandTrimmed.StartsWith("$GNGLL"))
                {
                    var data = commandTrimmed.Split(',');
                    var lat = data[1];
                    var latDir = data[2];
                    var lon = data[3];
                    var lonDir = data[4];
                    Latitude = ConvertToGeoLocation(lat, latDir);
                    Longitude = ConvertToGeoLocation(lon, lonDir);
                    LocationChanged?.Invoke(Latitude, Longitude);
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
                _serialDevice.Dispose();
                _serialDevice.DataReceived -= SerialDevice_DataReceived;
                _serialDevice = null;
            }
        }
    }
}