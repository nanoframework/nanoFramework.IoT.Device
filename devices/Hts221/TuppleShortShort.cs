// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hts221
{
    internal class TuppleShortShort
    {
        public TuppleShortShort(short s1, short s2)
        {
            S1 = s1;
            S2 = s2;
        }

        public short S1 { get; set; }

        public short S2 { get; set; }
    }
}
