using System;

using Iot.Device.ePaperGraphics;

namespace Iot.Device.ePaper.Buffers
{
    public abstract class FrameBufferBase : IFrameBuffer
    {
        /// <inheritdoc/>>
        public int Height { get; }

        /// <inheritdoc/>>
        public int Width { get; }

        /// <inheritdoc/>>
        public int BitDepth
            => this.ColorFormat switch
            {
                ColorFormat.Color1BitPerPixel => 1,
                ColorFormat.Color2BitPerPixel => 2,

                _ => throw new NotImplementedException()
            };

        /// <inheritdoc/>>
        public byte[] Buffer { get; }

        /// <inheritdoc/>>
        public int BufferByteCount 
            => (this.Width * this.Height) / 8;

        /// <inheritdoc/>
        public abstract ColorFormat ColorFormat { get; }

        public FrameBufferBase(int height, int width)
        {
            this.Height = height;
            this.Width = width;
            this.Buffer = new byte[this.BufferByteCount];
        }

        public FrameBufferBase(int height, int width, byte[] buffer)
        {
            this.Height = height;
            this.Width = width;

            if (buffer.Length != this.BufferByteCount)
                throw new ArgumentException("Length mismatch between the provided buffer and the specified width and height.");

            this.Buffer = buffer;
        }

        /// <inheritdoc/>>
        public void Clear()
        {
            Array.Clear(this.Buffer, 0, this.Buffer.Length);
        }

        /// <inheritdoc/>>
        public void CopyFrom(IFrameBuffer buffer)
            => this.CopyFrom(buffer, Point.Default);

        /// <inheritdoc/>>
        public virtual void CopyFrom(IFrameBuffer buffer, Point start)
            => this.SlowCopyFrom(buffer, start);

        /// <inheritdoc/>>
        public void Fill(Color color)
            => this.Fill(Point.Default, this.Width, this.Height, color);

        /// <inheritdoc/>>
        public abstract void Fill(Point start, int width, int height, Color color);

        /// <inheritdoc/>>
        public abstract Color GetPixel(Point point);

        /// <inheritdoc/>>
        public abstract void SetPixel(Point point, Color pixelColor);

        protected void SlowCopyFrom(IFrameBuffer buffer, Point start)
        {
            for (var x = start.X; x < buffer.Width; x++)
            {
                for (var y = start.Y; y < buffer.Height; y++)
                {
                    var currentPoint = new Point(x, y);
                    this.SetPixel(currentPoint, buffer.GetPixel(currentPoint));
                }
            }
        }
    }
}
