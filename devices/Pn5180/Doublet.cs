using System;
namespace Iot.Device.Pn5180
{
    public class Doublet
    {
        public Doublet(int bytes, int validBits)
        {
            Bytes = bytes;
            ValidBits = validBits;
        }

        public int Bytes { get; set; } 
        public int ValidBits { get; set; }
    }
}
