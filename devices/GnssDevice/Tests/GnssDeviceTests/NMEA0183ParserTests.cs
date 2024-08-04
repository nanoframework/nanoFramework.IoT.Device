// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Common.GnssDevice;
using nanoFramework.TestFramework;

namespace GnssDevice.Tests
{
    [TestClass]
    public class NMEA0183ParserTests
    {
        [TestMethod]
        [DataRow("$GNGSA,A,3,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20", (byte)GnssOperation.Auto, (byte)Fix.Fix3D)]
        [DataRow("$GNGSA,A,2,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20", (byte)GnssOperation.Auto, (byte)Fix.Fix2D)]
        [DataRow("$GNGSA,A,1,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20", (byte)GnssOperation.Auto, (byte)Fix.NoFix)]
        [DataRow("$GNGSA,M,1,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20", (byte)GnssOperation.Manual, (byte)Fix.NoFix)]
        public void ParseGngsa(string command, byte expectedMode, byte expectedFix)
        {
            // Act
            GngsaData result = (GngsaData)Nmea0183Parser.Parse(command);
            OutputHelper.WriteLine($"{(result == null ? "result null": "result not null")}");
            // Assert
            Assert.AreEqual(expectedMode, (byte)result.Mode);
            Assert.AreEqual(expectedFix, (byte)result.Fix);
        }

        [TestMethod]
        [DataRow("$GPGLL,5109.0262317,N,11401.8407304,W,202725.00,A,D*79", 51.1504372f, -114.03067884f)]
        public void ParseGpgll(string command, float expectedLatitude, float expectedLongitude)
        {
            // Act
            GpgllData result = (GpgllData)Nmea0183Parser.Parse(command);

            // Assert
            Assert.AreEqual((float)result.Location.Longitude, expectedLongitude);
            Assert.AreEqual((float)result.Location.Latitude, expectedLatitude);
        }
    }
}
