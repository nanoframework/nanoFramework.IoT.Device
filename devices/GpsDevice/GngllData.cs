// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GpsDevice
{
    public class GngllData
    {
        public GeoPosition Location { get; }

        public GngllData(GeoPosition location)
        {
            Location = location;
        }
    }
}