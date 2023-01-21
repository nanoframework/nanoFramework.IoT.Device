// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hts221
{
    internal class TuppleByteByte
    {
        public TuppleByteByte(byte b1, byte b2)
        {
            B1 = b1;
            B2 = b2;
        }

        public byte B1 { get; set; }

        public byte B2 { get; set; }
    }
}
