// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hts221
{
    internal class TuppleUshortUshort
    {
        public TuppleUshortUshort(ushort u1, ushort u2)
        {
            U1 = u1;
            U2 = u2;
        }

        public ushort U1 { get; set; }

        public ushort U2 { get; set; }
    }
}
