using System;

using Iot.Device.ePaper.Shared.Primitives;

namespace Iot.Device.ePaper.Shared.Buffers
{
    /// <summary>
    /// A display frame buffer implementation for tri-color displays with a separate buffer for the 3rd color.
    /// Wraps 2 <see cref="FrameBuffer1BitPerPixel"/>.
    /// </summary>
    public sealed class FrameBuffer2BitPerPixel : IFrameBuffer
    {
        /// <inheritdoc/>>
        public int Height { get; }

        /// <inheritdoc/>>
        public int Width { get; }

        /// <inheritdoc/>>
        public int BitDepth { get; } = 2;

        /// <inheritdoc/>
        public ColorFormat ColorFormat { get; } = ColorFormat.Color2BitPerPixel;

        /// <inheritdoc/>
        public byte[] Buffer 
            => throw new InvalidOperationException("Unified buffer is not available. Use BlackBuffer and ColorBuffer instead.");

        /// <inheritdoc/>>
        public int BufferByteCount
            => (this.Width * this.Height) / 8;

        /// <summary>
        /// Gets a buffer representing the Black/White frame.
        /// </summary>
        public FrameBuffer1BitPerPixel BlackBuffer { get; }

        /// <summary>
        /// Gets a buffer representing the color frame.
        /// </summary>
        public FrameBuffer1BitPerPixel ColorBuffer { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="FrameBuffer2BitPerPixel"/> class.
        /// </summary>
        /// <param name="height">The height of the frame to manage.</param>
        /// <param name="width">The width of the frame to manage.</param>
        public FrameBuffer2BitPerPixel(int height, int width)
        {
            this.BlackBuffer = new FrameBuffer1BitPerPixel(height, width);
            this.ColorBuffer = new FrameBuffer1BitPerPixel(height, width);
        }

        /// <inheritdoc/>
        public void Fill(Point start, int width, int height, Color color)
        {
            if (!this.IsPointWithinFrameBuffer(start))
                return;

            // if the current color isn't black or white, then we send data to the color buffer
            if (color != Color.Black
                && color != Color.White)
            {
                this.ColorBuffer.Fill(start, width, height, Color.White); // white will write 1 to the buffer
                this.BlackBuffer.Fill(start, width, height, Color.Black); // black writes 0s to the buffer
            }
            else if (color == Color.Black)
            {
                this.BlackBuffer.Fill(start, width, height, Color.Black);
                this.ColorBuffer.Fill(start, width, height, Color.Black); // black will write 0 to the buffer
            }
            else // white
            {
                this.BlackBuffer.Fill(start, width, height, Color.White);
                this.ColorBuffer.Fill(start, width, height, Color.Black); // black will write 0 to the buffer
            }
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException" />
        /// <remarks>
        /// This method is not implemented for this class as it is not possible 
        /// to convert a byte from <see cref="BlackBuffer"/> and <see cref="ColorBuffer"/>
        /// into a <see cref="Color"/> struct. We don't know what color <see cref="ColorBuffer"/>
        /// represents in the actual display hardware.
        /// </remarks>
        public Color GetPixel(Point point)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Checks if the specified pixel is set to 0 in the <see cref="BlackBuffer"/> and 1 in the <see cref="ColorBuffer"/>
        /// </summary>
        /// <param name="point">The pixel position.</param>
        /// <returns>True if pixel is a color other than black/white, otherwise; false.</returns>
        /// <remarks>Make sure to consider the result of <see cref="IsBlackPixel(Point)"/> as this is not enough on its own.</remarks>
        public bool IsColoredPixel(Point point)
            => this.ColorBuffer.GetPixel(point) == Color.White; // white indicates the pixel value is 1 (on)

        /// <summary>
        /// Checks if the specified pixel is set to 0 in the <see cref="BlackBuffer"/> and 0 in the <see cref="ColorBuffer"/>
        /// </summary>
        /// <param name="point">The pixel position.</param>
        /// <returns>True if pixel is black color, otherwise; false.</returns>
        public bool IsBlackPixel(Point point) 
            => this.IsColoredPixel(point) == false 
            && this.BlackBuffer.GetPixel(point) == Color.Black;

        /// <summary>
        /// Checks if the specified pixel is set to 1 in the <see cref="BlackBuffer"/> and 0 in the <see cref="ColorBuffer"/>
        /// </summary>
        /// <param name="point">The pixel position.</param>
        /// <returns>True if pixel is white color, otherwise; false.</returns>
        /// <remarks>Make sure to consider the result of <see cref="IsColoredPixel(Point)"/> as this is not enough on its own.</remarks>
        public bool IsWhitePixel(Point point)
            => this.IsBlackPixel(point) == false;

        /// <inheritdoc/>
        public void SetPixel(Point point, Color pixelColor)
        {
            if (!this.IsPointWithinFrameBuffer(point))
                return;

            // if the current color isn't black or white, then we send data to the color buffer
            if (pixelColor != Color.Black
                && pixelColor != Color.White)
            {
                this.ColorBuffer.SetPixel(point, Color.White); // white will write 1 to the buffer
            }
            else if (pixelColor == Color.Black)
            {
                this.BlackBuffer.SetPixel(point, pixelColor);
                this.ColorBuffer.SetPixel(point, pixelColor); // black will write 0 to the buffer
            }
            else // white
            {
                this.BlackBuffer.SetPixel(point, pixelColor);
                this.ColorBuffer.SetPixel(point, Color.Black); // black will write 0 to the buffer
            }
        }

        /// <inheritdoc/>
        public void WriteBuffer(IFrameBuffer buffer, Point start)
        {
            // if the frame is not the same type (different bit depth), use the slow copy method
            // because it converts every pixel properly.

            if (buffer is not FrameBuffer2BitPerPixel compatibleBuffer)
            {
                base.WriteBuffer(buffer, start);
                return;
            }

            // if the buffer is the same type (same bit depth), then we copy its contents directly into this instance
            this.BlackBuffer.WriteBuffer(compatibleBuffer.BlackBuffer, start);
            this.ColorBuffer.WriteBuffer(compatibleBuffer.ColorBuffer, start);
        }


        /// <summary>
        /// Checks if the specified coordinates fall within this frame buffer's area.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is within this frame buffer's area, otherwise; false.</returns>
        private bool IsPointWithinFrameBuffer(Point point)
        {
            return point.X >= 0 && point.X < this.Width
                && point.Y >= 0 && point.Y < this.Height;
        }
    }
}
