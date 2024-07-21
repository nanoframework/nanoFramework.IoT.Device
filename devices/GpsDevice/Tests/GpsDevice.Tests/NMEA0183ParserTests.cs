using Iot.Device.Common.GpsDevice;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GpsDevice.Tests
{
    [TestClass]
    public class NMEA0183ParserTests
    {
        [TestMethod]
        [DataRow("$GNGSA,A,3,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20", Mode.Auto, Fix.Fix3D)]
        [DataRow("$GNGSA,A,2,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20", Mode.Auto, Fix.Fix2D)]
        [DataRow("$GNGSA,A,1,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20", Mode.Auto, Fix.NoFix)]
        [DataRow("$GNGSA,M,1,65,67,80,81,82,88,66,,,,,,1.2,0.7,1.0*20", Mode.Manual, Fix.Fix3D)]
        public void ParseGngsa(string command, Mode expectedMode, Fix expectedFix)
        {
            // Act
            var result = NMEA0183Parser.ParseGngsa(command);

            // Assert
            Assert.AreEqual(expectedMode, result.Mode);
            Assert.AreEqual(expectedFix, result.Fix);
        }

        [TestMethod]
        [DataRow("$GPGLL,5109.0262317,N,11401.8407304,W,202725.00,A,D*79", 51.1504372d, -114.03067884d)]
        public void ParseGpgll(string command, double expectedLatitude, double expectedLongitude)
        {
            // Act
            var result = NMEA0183Parser.ParaseGngll(command);

            // Assert
            Assert.AreEqual(result.Location.Longitude, expectedLongitude);
            Assert.AreEqual(result.Location.Latitude, expectedLatitude);
        }
    }
}
