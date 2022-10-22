using Iot.Device.ePaper;

namespace Iot.Device.ePaperGraphics
{
    public sealed class Graphics
    {
        public IePaperDisplay ePaperDisplay { get; }

        public Graphics(IePaperDisplay ePaperDisplay)
        {
            this.ePaperDisplay = ePaperDisplay;
        }


    }
}
