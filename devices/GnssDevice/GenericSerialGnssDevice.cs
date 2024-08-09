// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO.Ports;
using System.Text;
using UnitsNet;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Implementation of a generic serial GNSS device.
    /// </summary>
    public class GenericSerialGnssDevice : GnssDevice, IDisposable
    {
        private readonly bool _shouldDispose;
        private SerialPort _serialPort;
        private bool _isrunning = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSerialGnssDevice"/> class.
        /// </summary>
        /// <param name="serialPort">A valid <see cref="SerialPort"/>. Note that you are responsible to set a watch char `\n` and provide the proper port configurations.</param>
        /// <param name="shouldDispose">True to dispose the serial port.</param>
        public GenericSerialGnssDevice(SerialPort serialPort, bool shouldDispose = true)
        {
            _serialPort = serialPort;
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericSerialGnssDevice"/> class.
        /// </summary>
        /// <param name="portName">Serial port name.</param>
        /// <param name="baudRate">Baud rate, default to 9600.</param>
        /// <param name="parity">PArity, default to None.</param>
        /// <param name="dataBits">Data bits, default to 8.</param>
        /// <param name="stopBits">Stop bits, default to One.</param>
        public GenericSerialGnssDevice(string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            _serialPort.ReadBufferSize = 2048;
            _serialPort.ReadTimeout = 2000;
            _serialPort.WatchChar = '\n';
            _shouldDispose = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Stop();
            if (_shouldDispose)
            {
                _serialPort?.Dispose();
                _serialPort = null;
            }
        }

        /// <inheritdoc/>
        public override string GetProductDetails()
        {
            return "Generic GNSS Serial device";
        }

        /// <inheritdoc/>
        public override bool Start()
        {
            if (_serialPort == null)
            {
                _isrunning = false;
                return false;
            }

            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
                _serialPort.DataReceived += SerialPortDataReceived;
            }

            _isrunning = true;
            return true;
        }

        /// <inheritdoc/>
        public override bool Stop()
        {
            _isrunning = false;
            if (_serialPort == null)
            {                
                return false;
            }

            if (_serialPort.IsOpen)
            {
                _serialPort.DataReceived -= SerialPortDataReceived;
                _serialPort.Close();
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool IsRunning => _isrunning;

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
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
                    var data = Nmea0183Parser.Parse(command);
                    if (data != null)
                    {
                        if (data is GgaData ggaData)
                        {
                            // GGA data has Latitude, Longitude, Accuracy (HDOP), Altitude, and Timestamp
                            // So update our location accoringly and raise the event if any except timestamp changed and Accuracy.
                            bool changed = false;
                            if (Location.Latitude != ggaData.Location.Latitude)
                            {
                                Location.Latitude = ggaData.Location.Latitude;
                                changed = true;
                            }

                            if (Location.Longitude != ggaData.Location.Longitude)
                            {
                                Location.Longitude = ggaData.Location.Longitude;
                                changed = true;
                            }

                            if (Location.Altitude != ggaData.Location.Altitude)
                            {
                                Location.Altitude = ggaData.Location.Altitude;
                                changed = true;
                            }

                            if (Location.Accuracy != ggaData.Location.Accuracy)
                            {
                                Location.Accuracy = ggaData.Location.Accuracy;
                            }

                            if (Location.Timestamp != ggaData.Location.Timestamp)
                            {
                                Location.Timestamp = ggaData.Location.Timestamp;
                            }

                            if (changed)
                            {
                                RaiseLocationChanged(Location);
                            }
                        }
                        else if (data is GsaData gsaData)
                        {
                            // GSA data has OperationMode, Fix, SatellitesInUse, PositionDilutionOfPrecision, HorizontalDilutionOfPrecision, VerticalDilutionOfPrecision
                            GnssOperation = gsaData.OperationMode;
                            Fix = gsaData.Fix;
                            SatellitesInUse = gsaData.SatellitesInUse;
                            if (Location.Accuracy != gsaData.PositionDilutionOfPrecision)
                            {
                                Location.Accuracy = gsaData.PositionDilutionOfPrecision;
                            }

                            if (Location.VerticalAccuracy != gsaData.VerticalDilutionOfPrecision)
                            {
                                Location.VerticalAccuracy = gsaData.VerticalDilutionOfPrecision;
                            }
                        }
                        else if (data is VtgData vtgData)
                        {
                            // VTG data has Course and Speed
                            bool changed = false;
                            if (Location.Course.Degrees != vtgData.Location.Course.Degrees)
                            {
                                Location.Course = vtgData.Location.Course;
                                changed = true;
                            }

                            if (Location.Speed.Knots != vtgData.Location.Speed.Knots)
                            {
                                Location.Speed = vtgData.Location.Speed;
                                changed = true;
                            }

                            if (changed)
                            {
                                RaiseLocationChanged(Location);
                            }
                        }
                        else if (data is RmcData rmcData)
                        {
                            // RMC data has Latitude, Longitude, Speed, Course, and Timestamp
                            bool changed = false;
                            if (Location.Latitude != rmcData.Location.Latitude)
                            {
                                Location.Latitude = rmcData.Location.Latitude;
                                changed = true;
                            }

                            if (Location.Longitude != rmcData.Location.Longitude)
                            {
                                Location.Longitude = rmcData.Location.Longitude;
                                changed = true;
                            }

                            if (Location.Speed.Knots != rmcData.Location.Speed.Knots)
                            {
                                Location.Speed = rmcData.Location.Speed;
                                changed = true;
                            }

                            if (Location.Course.Degrees != rmcData.Location.Course.Degrees)
                            {
                                Location.Course = rmcData.Location.Course;
                                changed = true;
                            }

                            if (Location.Timestamp != rmcData.Location.Timestamp)
                            {
                                Location.Timestamp = rmcData.Location.Timestamp;
                            }

                            if (changed)
                            {
                                RaiseLocationChanged(Location);
                            }
                        }
                        else if (data is GllData gllData)
                        {
                            // GLL data has Latitude, Longitude, and Timestamp
                            bool changed = false;
                            if (Location.Latitude != gllData.Location.Latitude)
                            {
                                Location.Latitude = gllData.Location.Latitude;
                                changed = true;
                            }

                            if (Location.Longitude != gllData.Location.Longitude)
                            {
                                Location.Longitude = gllData.Location.Longitude;
                                changed = true;
                            }

                            if (Location.Timestamp != gllData.Location.Timestamp)
                            {
                                Location.Timestamp = gllData.Location.Timestamp;
                            }

                            if (changed)
                            {
                                RaiseLocationChanged(Location);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                RaiseParsingError(exception);
            }
        }
    }
}
