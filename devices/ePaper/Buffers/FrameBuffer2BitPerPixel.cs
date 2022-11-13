// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Drawing;

using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper.Primitives;

namespace Iot.Device.EPaper.Buffers
{
    /// <summary>
    /// A display frame buffer implementation for tri-color displays with a separate buffer for the 3rd color.
    /// Wraps 2 <see cref="FrameBuffer1BitPerPixel"/> internally.
    /// </summary>
    public sealed class FrameBuffer2BitPerPixel : IFrameBuffer
    {
        private Point _startPoint;

        /// <inheritdoc/>>
        public int Height { get; }

        /// <inheritdoc/>>
        public int Width { get; }

        /// <inheritdoc/>
        public Point StartPoint
        {
            get => _startPoint;
            set
            {
                _startPoint = BlackBuffer.StartPoint = ColorBuffer.StartPoint = value;
            }
        }

        /// <inheritdoc/>>
        public int BitDepth { get; } = 2;

        /// <inheritdoc/>
        public byte[] Buffer
        {
            get
            {
                throw new InvalidOperationException("Unified buffer is not available. Use BlackBuffer and ColorBuffer instead.");
            }
        }

        /// <inheritdoc/>>
        /// <remarks>
        /// For this implementation of <see cref="IFrameBuffer"/>, this number represents the total number of bytes
        /// making up <see cref="BlackBuffer"/> and <see cref="ColorBuffer"/>
        /// </remarks>
        public int BufferByteCount
        {
            get
            {
                return BlackBuffer.BufferByteCount + ColorBuffer.BufferByteCount;
            }
        }

        /// <inheritdoc/>
        public ColorFormat ColorFormat { get; } = ColorFormat.Color2BitPerPixel;

        /// <inheritdoc/>
        public byte this[int index]
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public byte this[Point point]
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Gets a buffer representing the Black/White frame.
        /// </summary>
        public FrameBuffer1BitPerPixel BlackBuffer { get; }

        /// <summary>
        /// Gets a buffer representing the color frame.
        /// </summary>
        public FrameBuffer1BitPerPixel ColorBuffer { get; }

        /// <inheritdoc/>
        public int CurrentFramePage
        {
            get => BlackBuffer.CurrentFramePage;
            set => BlackBuffer.CurrentFramePage = ColorBuffer.CurrentFramePage = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrameBuffer2BitPerPixel"/> class.
        /// </summary>
        /// <param name="height">The height of the frame to manage.</param>
        /// <param name="width">The width of the frame to manage.</param>
        public FrameBuffer2BitPerPixel(int height, int width)
        {
            Height = height;
            Width = width;

            BlackBuffer = new FrameBuffer1BitPerPixel(height, width);
            ColorBuffer = new FrameBuffer1BitPerPixel(height, width);
        }

        /// <inheritdoc/>>
        public void Clear()
        {
            BlackBuffer.Clear(Color.White);
            ColorBuffer.Clear(Color.Black); // black sets it to 0s
        }

        /// <inheritdoc/>>
        public void Clear(Color color)
        {
            BlackBuffer.Clear(color);
            ColorBuffer.Clear(color);
        }

        /// <inheritdoc/>>
        public void Fill(Color color)
        {
            Fill(Point.Zero, Width, Height, color);
        }

        /// <inheritdoc/>
        public void Fill(Point start, int width, int height, Color color)
        {
            if (!IsPointWithinFrameBuffer(start))
            {
                return;
            }

            // if the current color isn't black or white, then we send data to the color buffer
            if (color != Color.Black
                && color != Color.White)
            {
                ColorBuffer.Fill(start, width, height, Color.White); // white will write 1 to the buffer
                BlackBuffer.Fill(start, width, height, Color.Black); // black writes 0s to the buffer
            }
            else if (color == Color.Black)
            {
                BlackBuffer.Fill(start, width, height, Color.Black);
                ColorBuffer.Fill(start, width, height, Color.Black); // black will write 0 to the buffer
            }
            else
            {
                // white
                BlackBuffer.Fill(start, width, height, Color.White);
                ColorBuffer.Fill(start, width, height, Color.Black); // black will write 0 to the buffer
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
        {
            return ColorBuffer.GetPixel(point) == Color.White; // white indicates the pixel value is 1 (on)
        }

        /// <summary>
        /// Checks if the specified pixel is set to 0 in the <see cref="BlackBuffer"/> and 0 in the <see cref="ColorBuffer"/>
        /// </summary>
        /// <param name="point">The pixel position.</param>
        /// <returns>True if pixel is black color, otherwise; false.</returns>
        public bool IsBlackPixel(Point point)
        {
            return IsColoredPixel(point) == false
                    && BlackBuffer.GetPixel(point) == Color.Black;
        }

        /// <summary>
        /// Checks if the specified pixel is set to 1 in the <see cref="BlackBuffer"/> and 0 in the <see cref="ColorBuffer"/>
        /// </summary>
        /// <param name="point">The pixel position.</param>
        /// <returns>True if pixel is white color, otherwise; false.</returns>
        /// <remarks>Make sure to consider the result of <see cref="IsColoredPixel(Point)"/> as this is not enough on its own.</remarks>
        public bool IsWhitePixel(Point point)
        {
            return IsBlackPixel(point) == false;
        }

        /// <inheritdoc/>
        public void SetPixel(Point point, Color pixelColor)
        {
            if (!IsPointWithinFrameBuffer(point))
            {
                return;
            }

            // if the current color isn't black or white, then we send data to the color buffer
            if (pixelColor != Color.Black
                && pixelColor != Color.White)
            {
                ColorBuffer.SetPixel(point, Color.White); // white will write 1 to the buffer
            }
            else if (pixelColor == Color.Black)
            {
                BlackBuffer.SetPixel(point, pixelColor);
                ColorBuffer.SetPixel(point, pixelColor); // black will write 0 to the buffer
            }
            else
            {
                // white
                BlackBuffer.SetPixel(point, pixelColor);
                ColorBuffer.SetPixel(point, Color.Black); // black will write 0 to the buffer
            }
        }

        /// <inheritdoc/>>
        public void WriteBuffer(IFrameBuffer buffer)
        {
            WriteBuffer(buffer, Point.Zero);
        }

        /// <inheritdoc/>>
        public void WriteBuffer(IFrameBuffer buffer, Point destinationStart)
        {
            WriteBuffer(buffer, Point.Zero, new Point(buffer.Width, buffer.Height), destinationStart);
        }

        /// <inheritdoc/>
        public void WriteBuffer(IFrameBuffer buffer, Point start, Point end, Point destinationStart)
        {
            if (buffer is FrameBuffer1BitPerPixel buffer1bpp)
            {
                // I think it is reasonable that a 1-bit deep buffer is copied over to the B/W buffer
                BlackBuffer.WriteBuffer(buffer1bpp, start, end, destinationStart);
            }
            else if (buffer is FrameBuffer2BitPerPixel buffer2bpp)
            {
                // if the buffer is the same type (same bit depth), then we copy its contents directly into this instance
                BlackBuffer.WriteBuffer(buffer2bpp.BlackBuffer, start, end, destinationStart);
                ColorBuffer.WriteBuffer(buffer2bpp.ColorBuffer, start, end, destinationStart);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public bool IsPointWithinFrameBuffer(Point point)
        {
            return point.X >= StartPoint.X && point.X < (StartPoint.X + Width)
                && point.Y >= StartPoint.Y && point.Y < (StartPoint.Y + Height);
        }

        /// <inheritdoc/>>
        public bool IsRangeWithinFrameBuffer(Point start, Point end)
        {
            return IsPointWithinFrameBuffer(start) && IsPointWithinFrameBuffer(end);
        }
    }
}
