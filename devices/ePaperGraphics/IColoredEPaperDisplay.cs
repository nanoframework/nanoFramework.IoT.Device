using Iot.Device.ePaper;

namespace Iot.Device.ePaperGraphics
{
    public interface IColoredEPaperDisplay : IePaperDisplay
    {
        void Clear(Color color);

        void DrawPixel(int x, int y, Color color);
    }
}
