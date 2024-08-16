// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.AtModem.Modem;
using Iot.Device.Common.GnssDevice;
using UnitsNet;

namespace Iot.Device.AtModem.Gnss
{
    /// <summary>
    /// Represents a <see cref="Sim800"/> Global Navigation Satellite System class.
    /// </summary>
    public class Sim800Gnss : GnssBase
    {
        private readonly ModemBase _modem;
        private GnssStartMode _startMode = GnssStartMode.Cold;
        private Location _position;
        private CancellationTokenSource _cs;
        private Timer _timer;
        private TimeSpan _timerTimeSpan;

        internal Sim800Gnss(ModemBase modem)
        {
            _modem = modem;
        }

        /// <inheritdoc/>
        public override bool IsRunning
        {
            get
            {
                AtResponse response = _modem.Channel.SendCommandReadSingleLine("AT+CGNSPWR?", "+CGNSPWR");
                if (response.Success)
                {
                    string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                    if (line.StartsWith("+CGNSPWR: "))
                    {
                        string power = line.Substring(10);
                        return power == "1";
                    }
                }

                return false;
            }
        }

        /// <inheritdoc/>
        public override bool Start()
        {
            bool success = false;
            AtResponse response = _modem.Channel.SendCommand("AT+CGNSPWR=1");
            success |= response.Success;
            return success;
        }

        /// <inheritdoc/>
        public override bool Stop()
        {
            bool success = false;
            AtResponse response = _modem.Channel.SendCommand("AT+CGNSPWR=0");
            success |= response.Success;
            return success;
        }

        /// <inheritdoc/>
        public override string GetProductDetails()
        {
            var version = string.Empty;
            var response = _modem.Channel.SendCommandReadSingleLine("AT+CGNSVER", string.Empty);
            if (response.Success)
            {
                version = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
            }

            return version;
        }

        /// <inheritdoc/>
        public override Location GetLocation()
        {
            ProcessLocation();
            return Location;
        }

        /// <inheritdoc/>
        public override TimeSpan AutomaticUpdate
        {
            get => throw new NotImplementedException();
            set
            {
                _timerTimeSpan = value;
                _timer = new Timer(TimerCallback, null, TimeSpan.Zero, _timerTimeSpan);
            }
        }

        private void ProcessLocation()
        {
            var response = _modem.Channel.SendCommandReadSingleLine("AT+CGNSINF", "+CGNSINF");
            if (response.Success)
            {
                var message = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                message = message.Substring(10);
                if (message.Length > 20)
                {
                    try
                    {
                        var elements = message.Split(',');

                        // <lat> Latitude of current position.Output format is dd.dddddd
                        double lat = Nmea0183Parser.ConvertToDouble(elements[3]);

                        // <log> Longitude of current position. Output format is ddd.dddddd
                        double lon = Nmea0183Parser.ConvertToDouble(elements[4]);

                        Location position = new Location(lat, lon);

                        // <speed> Speed Over Ground. Unit is km/H.
                        position.Speed = Speed.FromKilometersPerHour(Nmea0183Parser.ConvertToDouble(elements[6]));

                        // <course> Course. Degrees.
                        position.Course = Angle.FromDegrees(Nmea0183Parser.ConvertToDouble(elements[7]));

                        // yyyyMMddhhmmss.sss
                        position.Timestamp = new DateTime(
                            int.Parse(elements[2].Substring(0, 4)),
                            int.Parse(elements[2].Substring(4, 2)),
                            int.Parse(elements[2].Substring(6, 2)),
                            int.Parse(elements[2].Substring(8, 2)),
                            int.Parse(elements[2].Substring(10, 2)),
                            int.Parse(elements[2].Substring(12, 2)),
                            int.Parse(elements[2].Substring(15, 3)));

                        // <mode> Fix mode 2=2D fix 3=3D fix
                        Fix = elements[8] == "2" ? Fix.Fix2D : Fix.Fix3D;

                        // <alt> MSL Altitude. Unit is meters.
                        position.Altitude = Nmea0183Parser.ConvertToDouble(elements[5]);

                        // <PDOP> Position Dilution Of Precision.
                        position.Accuracy = Nmea0183Parser.ConvertToDouble(elements[11]);

                        // <VDOP> Vertical Dilution Of Precision.
                        position.VerticalAccuracy = Nmea0183Parser.ConvertToDouble(elements[12]);

                        // <NoSV> Number of satellites involved
                        SatellitesInView = new int[Nmea0183Parser.ConvertToInt(elements[16])];

                        _position = position;
                        if (_cs != null)
                        {
                            _cs.Cancel();
                        }

                        Location = position;
                    }
                    catch (Exception ex)
                    {
                        // Nothing on purpose
                        Debug.WriteLine($"GNSSPOSITION exception: {ex}");
                    }
                }
            }
        }

        private void TimerCallback(object sender)
        {
            ProcessLocation();
        }
    }
}
