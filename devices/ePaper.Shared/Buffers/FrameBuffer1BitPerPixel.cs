using System;

using Iot.Device.ePaper.Shared.Primitives;

namespace Iot.Device.ePaper.Shared.Buffers
{
    /// <summary>
    /// A display frame buffer implementation for dual-color displays.
    /// </summary>
    public class FrameBuffer1BitPerPixel : FrameBufferBase
    {
        /// <inheritdoc/>
        public override ColorFormat ColorFormat { get; } = ColorFormat.Color1BitPerPixel;

        /// <summary>
        /// Initializes a new instance of <see cref="FrameBuffer1BitPerPixel"/> class.
        /// </summary>
        /// <param name="height">The height of the frame to manage.</param>
        /// <param name="width">The width of the frame to manage.</param>
        public FrameBuffer1BitPerPixel(int height, int width)
            : base(height, width)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FrameBuffer1BitPerPixel"/> class by copying the specified buffer.
        /// </summary>
        /// <param name="height">The height of the frame to manage.</param>
        /// <param name="width">The width of the frame to manage.</param>
        /// <param name="buffer">The starting frame buffer.</param>
        public FrameBuffer1BitPerPixel(int height, int width, byte[] buffer)
            : base(height, width, buffer)
        {
        }

        /// <inheritdoc/>>
        public override void Clear(Color color)
        {
            var colorValue = color.Color1bpp;
            for (var i = 0; i < this.Buffer.Length; i++)
                this.Buffer[i] = colorValue;
        }

        /// <inheritdoc/>
        public override void Fill(Point start, int width, int height, Color color)
        {
            if (!this.IsPointWithinFrameBuffer(start))
                return;

            var colorVal = color.Color1bpp;

            for (var y = start.Y; y < start.Y + height; y++)
            {
                for (var x = start.X; x < start.X + width; x++)
                {
                    var frameBufferIndex = this.GetFrameBufferIndexForPoint(x ,y);

                    // improve performance by trying to set an entire byte at once
                    // if the current x position has 8 more columns ahead of it then set the whole byte
                    if ((x % 8) == 0 && (x + 8) <= width)
                    {
                        this[frameBufferIndex] = colorVal;

                        // move the x location by 7 (loop adds 1) so we can land on the next byte start
                        x += 7;
                    }
                    else
                    {
                        this.SetPixel(new Point(x, y), color);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override Color GetPixel(Point point)
        {
            var frameBufferIndex = this.GetFrameBufferIndexForPoint(point);

            return (this[frameBufferIndex] & GetPointByteMask(point)) != 0
                ? Color.White
                : Color.Black;
        }

        /// <inheritdoc/>
        public override void SetPixel(Point point, Color pixelColor)
        {
            if (!this.IsPointWithinFrameBuffer(point))
                return;

            var frameBufferIndex = this.GetFrameBufferIndexForPoint(point);

            if (pixelColor == Color.Black)
            {
                this[frameBufferIndex] &= (byte)~this.GetPointByteMask(point);
            }
            else
            {
                this[frameBufferIndex] |= this.GetPointByteMask(point);
            }
        }

        /// <inheritdoc/>
        public override void WriteBuffer(IFrameBuffer buffer, Point start, Point end, Point destinationStart)
        {
            // if the frame is not the same type (different bit depth), use the slow copy method
            // because it converts every pixel properly.

            if (buffer is not FrameBuffer1BitPerPixel otherBuffer)
            {
                base.WriteBuffer(buffer, start, end, destinationStart);
                return;
            }

            // if the buffer is the same type (same bit depth) then we copy
            // the content while paying attention to our (x,y) position
            for (var y = 0; y < end.Y; y++)
            {
                for (var x = 0; x < end.X; x++)
                {
                    var currentRelativePoint = new Point(x, y);
                    var sourceAbsolutePosition = start + currentRelativePoint;
                    var destinationAbsolutePosition = destinationStart + currentRelativePoint;

                    // improve performance by trying to copy an entire byte at once
                    // if the current x position has 8 more columns ahead of it then set the whole byte
                    //if ((x % 8) == 0 && (x + 8) <= otherBuffer.Width)
                    //{
                    //    this[destinationAbsolutePosition] = otherBuffer[sourceAbsolutePosition];

                    //    // move the x location by 7 (loop adds 1) so we can land on the next byte start
                    //    x += 7;
                    //}
                    //else
                    //{
                        this.SetPixel(destinationAbsolutePosition, 
                            otherBuffer.GetPixel(sourceAbsolutePosition));
                    //}
                }
            }
        }
    }
}
