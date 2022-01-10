// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Card.Mifare
{
    internal class Sixtet
    {
        public Sixtet(byte c1a, byte c1b, byte c2a, byte c2b, byte c3a, byte c3b)
        {
            C1a = c1a;
            C1b = c1b;
            C2a = c2a;
            C2b = c2b;
            C3a = c3a;
            C3b = c3b;
        }

        public byte C1a { get; set; }
        public byte C1b { get; set; }
        public byte C2a { get; set; }
        public byte C2b { get; set; }
        public byte C3a { get; set; }
        public byte C3b { get; set; }
    }
}
