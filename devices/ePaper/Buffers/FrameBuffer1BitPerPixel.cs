using Iot.Device.ePaperGraphics;

namespace Iot.Device.ePaper.Buffers
{
    public class FrameBuffer1BitPerPixel : FrameBufferBase
    {
        public override ColorFormat ColorFormat { get; } = ColorFormat.Color1BitPerPixel;

        public FrameBuffer1BitPerPixel(int height, int width)
            : base(height, width)
        {
        }

        public FrameBuffer1BitPerPixel(int height, int width, byte[] buffer)
            : base(height, width, buffer)
        {
        }

        public override void Fill(Point start, int width, int height, Color color)
        {
            throw new System.NotImplementedException();
        }

        public override Color GetPixel(Point point)
        {
            var frameBufferIndex = this.GetFrameBufferIndexForPoint(point);

            return (this.Buffer[frameBufferIndex] & GetPointByteMask(point)) != 0
                ? Color.Black
                : Color.White;
        }

        public override void SetPixel(Point point, Color pixelColor)
        {
            if (point.X < 0 || point.X >= this.Width || point.Y < 0 || point.Y >= this.Height)
                return;

            var frameBufferIndex = this.GetFrameBufferIndexForPoint(point);

        }

        protected int GetFrameBufferIndexForPoint(Point point) 
            => (point.X + (point.Y * this.Width)) / 8;

        protected byte GetPointByteMask(Point point)
            => (byte)(128 >> (point.X & 7));
    }
}
