// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.AtModem.Modem;
using Iot.Device.Common.GnssDevice;
using UnitsNet;

namespace Iot.Device.AtModem.Gnss
{
    /// <summary>
    /// Represents a <see cref="Sim7672"/> Global Navigation Satellite System class..
    /// </summary>
    public class GnssSim7672 : GnssDevice
    {
        private readonly ModemBase _modem;
        private GnssStartMode _startMode = GnssStartMode.Cold;
        private Sim7276Location _position;
        private CancellationTokenSource _cs;

        internal GnssSim7672(ModemBase modem)
        {
            _modem = modem;
            _modem.GenericEvent += GenericEvent;
        }

        private void GenericEvent(object sender, Events.GenericEventArgs e)
        {
            // Success:
            // +CGNSSINFO: 3,05,06,01,01,48.749920,N,2.297945,E,060324,132921.000,40.5,1.08,185.89,2.13,1.91,0.94,8
            // No fix:
            // +CGNSSINFO: ,,,,,,,,
            if (e.Message.StartsWith("+CGNSSINFO"))
            {
                _position = null;
                string message = e.Message.Substring(12);
                if (message.Length > 8)
                {
                    try
                    {
                        var elements = message.Split(',');

                        // <lat> Latitude of current position.Output format is dd.ddddd
                        float lat = float.Parse(elements[5]);

                        // <N/S> N/S Indicator, N=north or S=south.
                        lat = elements[6] == "N" ? lat : -lat;

                        // <log> Longitude of current position. Output format is ddd.ddddd
                        float lon = float.Parse(elements[7]);

                        // <E/W> E/W Indicator, E=east or W=west.
                        lon = elements[8] == "E" ? lon : -lon;
                        Sim7276Location position = new Sim7276Location();                        

                        // <mode> Fix mode 2=2D fix 3=3D fix
                        Fix = elements[0] == "2" ? Fix.Fix2D : Fix.Fix3D;

                        // <GPS-SVs> GPS satellite visible numbers
                        int sat = int.Parse(elements[1]);
                        position.GpsNumberVisibleSatellites = sat;

                        // <GLONASS-SVs> GLONASS satellite visible numbers
                        sat = int.Parse(elements[2]);
                        position.GlonassNumberVisibleSatellites = sat;

                        // <GALILEO -SVs> GALILEO satellite visible numbers
                        sat = int.Parse(elements[3]);
                        position.GalileoNumberVisibleSatellites = sat;

                        // <BEIDOU-SVs> BEIDOU satellite visible numbers
                        sat = int.Parse(elements[4]);
                        position.BeidouNumberVisibleSatellites = sat;

                        // <date> Date. Output format is ddmmyy.
                        // <UTC-time> UTC Time. Output format is hhmmss.ss.
                        position.DateTime = new DateTime(
                            int.Parse(elements[9].Substring(4, 2)) + 2000,
                            int.Parse(elements[9].Substring(2, 2)),
                            int.Parse(elements[9].Substring(0, 2)),
                            int.Parse(elements[10].Substring(0, 2)),
                            int.Parse(elements[10].Substring(2, 2)),
                            int.Parse(elements[10].Substring(4, 2)),
                            int.Parse(elements[10].Substring(7, 2)));

                        // <alt> MSL Altitude. Unit is meters.
                        pos = float.Parse(elements[11]);
                        position.Altitude = pos;

                        // <speed> Speed Over Ground. Unit is knots.
                        pos = float.Parse(elements[12]);
                        position.Speed = pos;

                        // <course> Course. Degrees.
                        pos += float.Parse(elements[13]);
                        position.Course = Angle.FromDegrees(pos);

                        // <PDOP> Position Dilution Of Precision.
                        pos = float.Parse(elements[14]);
                        position.PositionDilutionOfPrecision = pos;

                        // <HDOP> Horizontal Dilution Of Precision.
                        pos = float.Parse(elements[15]);
                        position.HorizontalDilutionOfPrecision = pos;

                        // <VDOP> Vertical Dilution Of Precision.
                        pos = float.Parse(elements[16]);
                        position.VerticalDilutionOfPrecision = pos;

                        // <NoSV> Number of satellites involved
                        sat = int.Parse(elements[17]);
                        position.TotalNumberOfSatellitesUsed = sat;

                        _position = position;
                        if (_cs != null)
                        {
                            _cs.Cancel();
                        }

                        OnGnssPositionUpdate(new Events.GnssPositionArgs(position));
                    }
                    catch (Exception ex)
                    {
                        // Nothing on purpose
                        Debug.WriteLine($"GNSSPOSITION exception: {ex}");
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override bool Start()
        {
            bool success = false;
            AtResponse response = _modem.Channel.SendCommand("AT+CGNSSPWR=1");
            success |= response.Success;
            response = _modem.Channel.SendCommand("AT+CGNSSTST=1");
            success |= response.Success;
            return success;
        }

        /// <inheritdoc/>
        public override bool IsRunning
        {
            get
            {
                AtResponse response = _modem.Channel.SendCommandReadSingleLine("AT+CGNSSPWR?", "+CGNSSPWR");
                if (response.Success)
                {
                    string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                    if (line.StartsWith("+CGNSSPWR: "))
                    {
                        string power = line.Substring(11);
                        return power == "1";
                    }
                }

                return false;
            }
        }

        /// <inheritdoc/>
        public override string GetProductDetails()
        {
            AtResponse response = _modem.Channel.SendCommandReadSingleLine("AT+CGNSSPROD", "+CGNSSPROD");
            if (response.Success)
            {
                string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                if (line.StartsWith("+CGNSSPROD: "))
                {
                    string power = line.Substring(12);
                    return power;
                }
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override bool Stop()
        {
            AtResponse response = _modem.Channel.SendCommand("AT+CGNSSPWR=0");
            return response.Success;
        }

        /// <inheritdoc/>
        public override GnssPosition GetLocation()
        {
            _cs = new CancellationTokenSource(2000);
            _modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes("AT+CGNSSINFO\r\n"));
            _cs.Token.WaitHandle.WaitOne(2000, true);

            return _position;
        }

        /// <inheritdoc/>
        public override GnssStartMode StartMode
        {
            // We don't have a way to check that value, assuming Cold is the default
            get => _startMode;
            set
            {
                switch (value)
                {
                    default:
                    case GnssStartMode.Cold:
                        _modem.Channel.SendCommand("AT+CGPSCOLD");
                        break;
                    case GnssStartMode.Warm:
                        _modem.Channel.SendCommand("AT+CGPSWARM");
                        break;
                    case GnssStartMode.Hot:
                        _modem.Channel.SendCommand("AT+CGPSHOT");
                        break;
                }

                _startMode = value;
            }
        }

        /// <inheritdoc/>
        public override GnssMode GnssMode
        {
            get
            {
                GnssMode gnssMode = GnssMode.None;
                AtResponse response = _modem.Channel.SendCommandReadSingleLine("AT+CGNSSMODE?", "+CGNSSMODE");
                if (response.Success)
                {
                    string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                    if (line.StartsWith("+CGNSSMODE: "))
                    {
                        string mode = line.Substring(12);
                        switch (mode)
                        {
                            case "1":
                                gnssMode = GnssMode.Gps;
                                break;
                            case "3":
                                gnssMode = GnssMode.Gps | GnssMode.Glonass;
                                break;
                            case "5":
                                gnssMode = GnssMode.Gps | GnssMode.Galileo;
                                break;
                            case "9":
                                gnssMode = GnssMode.Gps | GnssMode.Bds;
                                break;
                            case "13":
                                gnssMode = GnssMode.Gps | GnssMode.Galileo | GnssMode.Bds;
                                break;
                            case "15":
                                gnssMode = GnssMode.Gps | GnssMode.Glonass | GnssMode.Galileo | GnssMode.Bds;
                                break;
                            default:
                                break;
                        }
                    }
                }

                return gnssMode;
            }

            set
            {
                int val = 0;
                if (value.HasFlag(GnssMode.Gps))
                {
                    val |= 0b0000_0001;
                }

                if (value.HasFlag(GnssMode.Glonass))
                {
                    val |= 0b0000_00010;
                }

                if (value.HasFlag(GnssMode.Galileo))
                {
                    val |= 0b0000_0100;
                }

                if (value.HasFlag(GnssMode.Bds))
                {
                    val |= 0b0000_1000;
                }

                _modem.Channel.SendCommand($"AT+CGNSSMODE={val}");
            }
        }

        /// <inheritdoc/>
        public override TimeSpan AutomaticUpdate
        {
            get
            {
                AtResponse response = _modem.Channel.SendCommandReadSingleLine("AT+CGNSSINFO?", "+CGNSSINFO");
                if (response.Success)
                {
                    string line = response.Intermediates.Count > 0 ? (string)response.Intermediates[0] : string.Empty;
                    if (line.StartsWith("+CGNSSINFO: "))
                    {
                        string power = line.Substring(12);
                        if (int.TryParse(power, out int val))
                        {
                            // Those are seconds
                            return TimeSpan.FromSeconds(val);
                        }
                    }
                }

                return TimeSpan.Zero;
            }

            set
            {
                _modem.Channel.SendCommand($"AT+CGNSSINFO={(int)value.TotalSeconds}");
            }
        }
    }
}
