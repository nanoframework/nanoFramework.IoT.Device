using Iot.Device.ePaper;

namespace Iot.Device.ePaperGraphics
{
    public interface IColoredEPaperDisplay : IePaperDisplay
    {
        void DrawPixel(int x, int y, Color color);

        void DrawColorBuffer(byte[] bitmap, int startXPos, int startYPos);
    }
}
