// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// DhtSensor supports DHT familly sensors
    /// </summary>
    public class DhtSensor
    {
        /// <summary>
        /// Only Digital ports including the analogic sensors (A0 = D14, A1 = D15, A2 = D16)
        /// </summary>
        public static ListGrovePort SupportedPorts => new ListGrovePort()
        {
            GrovePort.DigitalPin2,
            GrovePort.DigitalPin3,
            GrovePort.DigitalPin4,
            GrovePort.DigitalPin5,
            GrovePort.DigitalPin6,
            GrovePort.DigitalPin7,
            GrovePort.DigitalPin8,
            GrovePort.DigitalPin14,
            GrovePort.DigitalPin15,
            GrovePort.DigitalPin16
        };

        private readonly double[] _lastTemHum = new double[2] { double.NaN, double.NaN };
        private GrovePi _grovePi;

        /// <summary>
        /// grove sensor port
        /// </summary>
        private GrovePort _port;

        /// <summary>
        /// Initialize the DHT Sensor class
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        /// <param name="dhtType">The DHT type</param>
        public DhtSensor(GrovePi grovePi, GrovePort port, DhtType dhtType)
        {
            if (!SupportedPorts.Contains(port))
            {
                throw new ArgumentException(nameof(port), "Grove port not supported");
            }

            _grovePi = grovePi;
            DhtType = dhtType;
            _port = port;
            // Ask for the temperature so we will have one in cache
            _grovePi.WriteCommand(GrovePiCommand.DhtTemp, _port, (byte)DhtType, 0);
        }

        /// <summary>
        /// Get the type of DHT sensor
        /// </summary>
        public DhtType DhtType { get; internal set; }

        /// <summary>
        /// Get an array of 2 double
        /// First is the temperature in degree Celsius
        /// Second is the relative humidity from 0.0 to 100.0
        /// </summary>
        public double[] Value
        {
            get
            {
                Read();
                return _lastTemHum;
            }
        }

        /// <summary>
        /// You need to read the sensorbefore getting the value
        /// </summary>
        public void Read()
        {
            // Fix for the firmware 1.4.0, you can request a new measurement only if you got data
            // from the previous one
            if (!(double.IsNaN(_lastTemHum[0]) && double.IsNaN(_lastTemHum[1])))
            {
                _grovePi.WriteCommand(GrovePiCommand.DhtTemp, _port, (byte)DhtType, 0);
            }

            // Wait a little bit to read the result
            // Delay from source code, line 90 in the Grove Pi firmware repository
            // This is needed for firmware 1.4.0 and also working for previous versions
            Thread.Sleep(300);
            var retArray = _grovePi.ReadCommand(GrovePiCommand.DhtTemp, _port);
            _lastTemHum[0] = BitConverter.ToSingle(retArray!, 1);
            _lastTemHum[1] = BitConverter.ToSingle(retArray!, 5);
        }

        /// <summary>
        /// Returns the temperature and humidity in a formated way
        /// </summary>
        /// <returns>Returns the temperature and humidity in a formated way</returns>
        public override string ToString() => $"Temperature: {_lastTemHum[0].ToString("0.00")} °C, Humidity: {_lastTemHum[1].ToString("0.00")} %";

        /// <summary>
        /// Get the last temperature measured in Farenheit
        /// </summary>
        public double LastTemperatureInFarenheit => _lastTemHum[0] * 9 / 5 + 32;

        /// <summary>
        /// Get the last temperature measured in Celsius
        /// </summary>
        public double LastTemperature => _lastTemHum[0];

        /// <summary>
        /// Get the last measured relative humidy from 0.0 to 100.0
        /// </summary>
        public double LastRelativeHumidity => _lastTemHum[1];

        /// <summary>
        /// Get the name of the DHT sensor
        /// </summary>
        public string SensorName => $"{DhtType} Temperature and Humidity Sensor";
    }
}
