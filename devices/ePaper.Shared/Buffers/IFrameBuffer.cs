// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using Iot.Device.ePaper.Shared.Primitives;

namespace Iot.Device.ePaper.Shared.Buffers
{
    /// <summary>
    /// Represents a display frame buffer.
    /// </summary>
    public interface IFrameBuffer
    {
        /// <summary>
        /// Gets the width of the buffer in pixel.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the height of the buffer in pixel.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets or sets the position from which this frame's area starts.
        /// </summary>
        Point StartPoint { get; set; }

        /// <summary>
        /// Gets or sets the current frame page index which this frame instance is covering.
        /// </summary>
        int CurrentFramePage { get; set; }

        /// <summary>
        /// Gets the bit depth of the frame.
        /// </summary>
        int BitDepth { get; }

        /// <summary>
        /// Gets the color format used across this frame buffer.
        /// </summary>
        ColorFormat ColorFormat { get; }

        /// <summary>
        /// Gets the internal buffer representing this frame.
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// Gets the total number of bytes that the current buffer is made of.
        /// </summary>
        int BufferByteCount { get; }

        /// <summary>
        /// Gets or sets a byte from the <see cref="Buffer"/> using the specified index value.
        /// </summary>
        /// <param name="index">The index of the <see cref="byte"/> to get.</param>
        /// <returns>A <see cref="byte"/> from <see cref="Buffer"/></returns>
        byte this[int index] { get; set; }

        /// <summary>
        /// Gets or sets a byte from the <see cref="Buffer"/> using the specified point.
        /// </summary>
        /// <param name="point">The point to get the byte containing the pixel.</param>
        /// <returns>A byte that contains the pixel specified by the point.</returns>
        byte this[Point point] { get;set; }

        /// <summary>
        /// Gets the pixel at the specified position.
        /// </summary>
        /// <param name="point">The position to get the pixel from.</param>
        /// <returns><see cref="Color"/> representing the specified pixel.</returns>
        Color GetPixel(Point point);

        /// <summary>
        /// Sets the value of the pixel at the specified position.
        /// </summary>
        /// <param name="point">The position to set the pixel.</param>
        /// <param name="pixelColor">The pixel color value to use.</param>
        void SetPixel(Point point, Color pixelColor);

        /// <summary>
        /// Fill the entire frame buffer using the specified color.
        /// </summary>
        /// <param name="color"></param>
        void Fill(Color color);

        /// <summary>
        /// Fill the specific portion of the frame buffer with the specified color.
        /// </summary>
        /// <param name="start">The starting position.</param>
        /// <param name="width">The width of the area.</param>
        /// <param name="height">The height of the area.</param>
        /// <param name="color">The color value to use.</param>
        void Fill(Point start, int width, int height, Color color);

        /// <summary>
        /// Copies the entire specified <see cref="IFrameBuffer"/> into the current frame buffer.
        /// </summary>
        /// <param name="buffer">The <see cref="IFrameBuffer" /> to copy from.</param>
        void WriteBuffer(IFrameBuffer buffer);

        /// <summary>
        /// Copies the specified <see cref="IFrameBuffer"/> into the current frame buffer.
        /// </summary>
        /// <param name="buffer">The <see cref="IFrameBuffer" /> to copy from.</param>
        /// <param name="destinationStart">The start point to begin writing to.</param>
        void WriteBuffer(IFrameBuffer buffer, Point destinationStart);

        /// <summary>
        /// Copies the specified <see cref="IFrameBuffer"/> into the current frame buffer.
        /// </summary>
        /// <param name="buffer">The <see cref="IFrameBuffer" /> to copy from.</param>
        /// <param name="start">The start position to copy from.</param>
        /// <param name="end">The point at which copying from the buffer will stop.</param>
        /// <param name="destinationStart">The start point to begin writing to.</param>
        void WriteBuffer(IFrameBuffer buffer, Point start, Point end, Point destinationStart);

        /// <summary>
        /// Resets the current frame buffer to its default starting values (all pixels set to 0).
        /// </summary>
        void Clear();

        /// <summary>
        /// Resets the current frame buffer to with its default starting values set to the specified color.
        /// </summary>
        void Clear(Color color);

        /// <summary>
        /// Checks if the specified point falls within the area this buffer is covering.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is within this frame's area, otherwise; false.</returns>
        bool IsPointWithinFrameBuffer(Point point);

        /// <summary>
        /// Checks if the specified range of points is within the current frame buffer.
        /// </summary>
        /// <param name="start">The starting point of the range.</param>
        /// <param name="end">The end point of the range.</param>
        /// <returns>True if the range is within this frame's area, otherwise; false.</returns>
        bool IsRangeWithinFrameBuffer(Point start, Point end);
    }
}
