using Iot.Device.ePaper;

namespace Iot.Device.ePaperGraphics
{
    public interface IColoredEPaperDisplay : IePaperDisplay
    {
        void DrawPixel(int x, int y, Color color);

        void DrawColorBuffer(int startXPos, int startYPos, params byte[] bitmap);

        void DirectDrawColorBuffer(int startXPos, int startYPos, params byte[] bitmap);
    }
}
