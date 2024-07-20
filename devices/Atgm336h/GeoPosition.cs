// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Atgm336h
{
    public class GeoPosition
    {
        public double Latitude { get; private set; }
        
        public double Longitude { get; private set; }

        public static GeoPosition FromDecimalDegrees(double latitude, double longitude)
        {
            return new GeoPosition()
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }
    }
}