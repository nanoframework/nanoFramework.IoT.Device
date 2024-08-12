// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Common.GnssDevice
{
    /// <summary>
    /// Base class for all Gnss devices.
    /// </summary>
    public abstract class GnssDevice
    {
        private Fix _fix = Fix.NoFix;
        private GnssOperation _mode = GnssOperation.Unknown;
        private Location _location = new Location();

        /// <summary>
        /// Delegate type to handle the event when the Gnss module fix status changes.
        /// </summary>
        /// <param name="fix">The new fix status.</param>
        public delegate void FixChangedHandler(Fix fix);

        /// <summary>
        /// Delegate type to handle the event when the Gnss module mode changes.
        /// </summary>
        /// <param name="mode">The new Gnss module mode.</param>
        public delegate void ModeChangedHandler(GnssOperation mode);

        /// <summary>
        /// Delegate type to handle the event when the Gnss module location changes.
        /// </summary>
        /// <param name="position">The new position.</param>
        public delegate void LocationChangeHandler(Location position);

        /// <summary>
        /// Delegate for the error handler when parsing the GPS data.
        /// </summary>
        /// <param name="exception">The exception that occurred during parsing.</param>
        public delegate void ParsingErrorHandler(Exception exception);

        /// <summary>
        /// Delegate type to handle parsed message that is not processed by based class.
        /// </summary>
        /// <param name="data">A <see cref="NmeaData"/> element.</param>
        public delegate void ParsedMessageHandler(NmeaData data);

        /// <summary>
        /// Delegate type to handle the event when the Gnss module receives an unparsed message.
        /// </summary>
        /// <param name="message">The unparsed message.</param>
        public delegate void UnparsedMessageHangler(string message);

        /// <summary>
        /// Represents the event handler for when the fix status of the Gnss module changes.
        /// </summary>
        public event FixChangedHandler FixChanged;

        /// <summary>
        /// Event that occurs when the location changes.
        /// </summary>
        public event LocationChangeHandler LocationChanged;

        /// <summary>
        /// Represents the event that is raised when the mode of the Gnss module is changed.
        /// </summary>
        public event ModeChangedHandler OperationModeChanged;

        /// <summary>
        /// Event handler for parsing errors that occur during data processing of GNSS module.
        /// </summary>
        public event ParsingErrorHandler ParsingError;

        /// <summary>
        /// Event handler for parsed message not handled by the base class.
        /// </summary>
        public event ParsedMessageHandler ParsedMessage;

        /// <summary>
        /// Event handler for unparsed messages.
        /// </summary>
        public event UnparsedMessageHangler UnparsedMessage;

        /// <summary>
        /// Gets or sets the fix status of the Gnss module.
        /// </summary>
        public Fix Fix
        {
            get => _fix;
            protected set
            {
                if (_fix == value)
                {
                    return;
                }

                _fix = value;
                FixChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Gets or sets the mode of the GNSS device.
        /// </summary>
        public virtual GnssMode GnssMode { get; set; }

        /// <summary>
        /// Gets or sets the mode of the Gnss module.
        /// </summary>
        /// <value>
        /// The mode of the Gnss module.
        /// </value>
        public GnssOperation GnssOperation
        {
            get => _mode;
            protected set
            {
                if (value == _mode)
                {
                    return;
                }

                _mode = value;
                OperationModeChanged?.Invoke(_mode);
            }
        }

        /// <summary>
        /// Gets or sets the last known location.
        /// </summary>
        public Location Location
        {
            get => _location;
            protected set
            {
                if (_location == value)
                {
                    return;
                }

                _location = value;
                LocationChanged?.Invoke(_location);
            }
        }

        /// <summary>
        /// Gets or sets the satellites in view.
        /// </summary>
        public int[] SatellitesInView { get; protected set; }

        /// <summary>
        /// Starts the GNSS device.
        /// </summary>
        /// <returns>A value indicating whether the start was successful.</returns>
        public abstract bool Start();

        /// <summary>
        /// Stops the GNSS device.
        /// </summary>
        /// <returns>A value indicating whether the stop was successful.</returns>
        public abstract bool Stop();

        /// <summary>
        /// Gets a value indicating whether the GNSS device is running.
        /// </summary>
        public abstract bool IsRunning 
        {
            get;
        }

        /// <summary>
        /// Gets the product details.
        /// </summary>
        /// <returns>A string representing the product details.</returns>
        public abstract string GetProductDetails();

        internal void ProcessCommands(string command)
        {
            var data = Nmea0183Parser.Parse(command);
            if (data == null)
            {
                // Here, we raised the unparsed event.
                RaiseUnparsedMessage(command);
                return;
            }

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
                SatellitesInView = gsaData.SatellitesInUse;
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
                if (Location.Course.Value != vtgData.Location.Course.Value)
                {
                    Location.Course = vtgData.Location.Course;
                    changed = true;
                }

                if (Location.Speed.Value != vtgData.Location.Speed.Value)
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

                if (Location.Speed.Value != rmcData.Location.Speed.Value)
                {
                    Location.Speed = rmcData.Location.Speed;
                    changed = true;
                }

                if (Location.Course.Value != rmcData.Location.Course.Value)
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
            else
            {
                // Pass any other processed NMEA message to the subscriber.
                // Usefull for heritage of this base class.
                RaiseParsedMessage(data);
            }
        }

        internal void RaiseParsingError(Exception ex) => ParsingError?.Invoke(ex);

        internal void RaiseFixChanged(Fix fix) => FixChanged?.Invoke(fix);

        internal void RaiseOperationModeChanged(GnssOperation mode) => OperationModeChanged?.Invoke(mode);

        internal void RaiseLocationChanged(Location position) => LocationChanged?.Invoke(position);

        internal void RaiseParsedMessage(NmeaData data) => ParsedMessage?.Invoke(data);

        internal void RaiseUnparsedMessage(string message) => UnparsedMessage?.Invoke(message);
    }
}