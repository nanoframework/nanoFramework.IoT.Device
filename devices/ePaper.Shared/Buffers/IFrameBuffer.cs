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
        /// Copies the specified <see cref="IFrameBuffer"/> into the current frame buffer.
        /// </summary>
        /// <param name="buffer">The <see cref="IFrameBuffer" /> to copy from.</param>
        void WriteBuffer(IFrameBuffer buffer);

        /// <summary>
        /// Copies the specified <see cref="IFrameBuffer"/> into the current frame buffer.
        /// </summary>
        /// <param name="buffer">The <see cref="IFrameBuffer" /> to copy from.</param>
        /// <param name="start">The start position to copy from.</param>
        void WriteBuffer(IFrameBuffer buffer, Point start);

        /// <summary>
        /// Resets the current frame buffer to its default start (all pixels set to 0).
        /// </summary>
        void Clear();
    }
}
