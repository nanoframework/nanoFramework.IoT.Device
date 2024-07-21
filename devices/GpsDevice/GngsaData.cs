// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Common.GpsDevice
{
    public class GngsaData
    {
        public Mode Mode { get; }

        public Fix Fix { get; }

        public GngsaData(Mode mode, Fix fix)
        {
            Mode = mode;
            Fix = fix;
        }
    }
}