namespace Iot.Device.ePaperGraphics
{
    public interface IFont
    {
        byte[] this[char character] { get; }

        int Height { get; }

        int Width { get; }
    }
}