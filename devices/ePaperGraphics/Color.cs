namespace Iot.Device.ePaperGraphics
{
    public struct Color
    {
        public byte R { get; private set; }

        public byte G { get; private set; }

        public byte B { get; private set; }

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static Color Black = new(r: 0, g: 0, b: 0);
        public static Color White = new(r: 255, g: 255, b: 255);
    }
}
