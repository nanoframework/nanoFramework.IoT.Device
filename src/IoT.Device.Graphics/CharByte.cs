using System;

namespace Iot.Device.Graphics
{
    internal class CharByte
    {
        public CharByte(char cr, byte bt)
        {
            Cr = cr;
            Bt = bt;
        }

        public char Cr { get; set; }
        public byte Bt { get; set; }
    }
}
