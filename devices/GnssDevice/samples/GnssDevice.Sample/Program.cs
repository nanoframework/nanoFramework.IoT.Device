// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common.GnssDevice;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Threading;

namespace GnssDevice.Sample
{
    public class Program
    {
        private static GenericSerialGnssDevice _gnssDevice;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework GNSS device!");

            // Configure GPIOs 16 and 17 to be used in UART2 (that's refered as COM3)
            Configuration.SetPinFunction(9, DeviceFunction.COM2_RX);
            Configuration.SetPinFunction(8, DeviceFunction.COM2_TX);

            // Add the TXT parser in the NMEA Parser
            Nmea0183Parser.AddParser(new TxtData());

            _gnssDevice = new GenericSerialGnssDevice("COM2");
            _gnssDevice.FixChanged += FixChanged;
            _gnssDevice.LocationChanged += LocationChanged;
            _gnssDevice.OperationModeChanged += OperationModeChanged;
            _gnssDevice.ParsingError += ParsingError;
            _gnssDevice.ParsedMessage += ParsedMessage;
            _gnssDevice.UnparsedMessage += UnparsedMessage;

            _gnssDevice.Start();

            Thread.Sleep(Timeout.Infinite);
        }

        private static void UnparsedMessage(string message)
        {
            Console.WriteLine($"Received unparsed message: {message}");
        }

        private static void ParsedMessage(NmeaData data)
        {
            Console.WriteLine($"Received parsed message: {data.GetType()}");
            if (data is TxtData txtData)
            {
                Console.WriteLine($"Received TXT message: {txtData.Text}, severity: {txtData.Severity}");
            }
        }

        private static void ParsingError(Exception exception)
        {
            Console.WriteLine($"Received parsed error: {exception.Message}");
        }

        private static void OperationModeChanged(GnssOperation mode)
        {
            Console.WriteLine($"Received Operation Mode changed: {mode}");
        }

        private static void LocationChanged(Location position)
        {
            Console.WriteLine($"Received position changed: {position.Latitude},{position.Longitude}");
        }

        private static void FixChanged(Fix fix)
        {
            Console.WriteLine($"Received Fix changed: {fix}");
        }
    }
}
