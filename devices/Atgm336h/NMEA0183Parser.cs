// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Atgm336h
{
    public static class NMEA0183Parser
    {
        public static GngllData ParaseGngll(string inputData)
        {
            var data = inputData.Split(',');
            var lat = data[1];
            var latDir = data[2];
            var lon = data[3];
            var lonDir = data[4];
            var latitude = ConvertToGeoLocation(lat, latDir);
            var longitude = ConvertToGeoLocation(lon, lonDir);

            return new GngllData(GeoPosition.FromDecimalDegrees(latitude, longitude));
        }

        public static GngsaData ParseGngsa(string inputData)
        {
            var data = inputData.Split(',');
            var mode = ConvertToMode(data[1]);
            var fix = ConvertToFix(data[2]);

            return new GngsaData(mode, fix);
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
    }
}