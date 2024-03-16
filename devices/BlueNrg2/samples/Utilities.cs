// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Samples
{
    internal static class Utilities
    {
        internal static int GetPinNumber(char port, byte pin)
        {
            if (port is < 'A' or > 'J' || pin > 15)
                throw new ArgumentException("port has to be between 'A' and 'J', pin has to be in range [0,15]");

            return port - 'A' << 4 | pin & 0x0F;
        }
    }
}
