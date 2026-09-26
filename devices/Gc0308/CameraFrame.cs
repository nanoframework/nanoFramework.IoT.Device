// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device.Gc0308
{
    /// <summary>
    /// Represents a single captured frame from the GC0308 camera sensor.
    /// Provides pixel access for camera frame data in RGB565 or YCbCr 4:2:2 format.
    /// </summary>
    public class CameraFrame
    {
        private readonly byte[] _data;
        private readonly OutputFormat _format;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraFrame"/> class.
        /// </summary>
        /// <param name="data">Raw pixel data from the camera.</param>
        /// <param name="width">Frame width in pixels.</param>
        /// <param name="height">Frame height in pixels.</param>
        /// <param name="stride">Number of bytes per row.</param>
        /// <param name="format">The pixel format of the frame data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
        public CameraFrame(byte[] data, int width, int height, int stride, OutputFormat format)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            Width = width;
            Height = height;
            Stride = stride;
            _format = format;
        }

        /// <summary>
        /// Gets the raw pixel data.
        /// </summary>
        public byte[] Data => _data;

        /// <summary>
        /// Gets the frame width in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the frame height in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets the number of bytes per row.
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// Gets the pixel format of this frame.
        /// </summary>
        public OutputFormat Format => _format;

        /// <summary>
        /// Creates a <see cref="CameraFrame"/> from RGB565 pixel data.
        /// </summary>
        /// <param name="data">Raw RGB565 data (2 bytes per pixel).</param>
        /// <param name="width">Frame width in pixels.</param>
        /// <param name="height">Frame height in pixels.</param>
        /// <returns>A new <see cref="CameraFrame"/> instance.</returns>
        public static CameraFrame FromRgb565(byte[] data, int width, int height)
        {
            return new CameraFrame(data, width, height, width * 2, OutputFormat.Rgb565);
        }

        /// <summary>
        /// Creates a <see cref="CameraFrame"/> from YCbCr 4:2:2 pixel data.
        /// </summary>
        /// <param name="data">Raw YCbCr 4:2:2 data (2 bytes per pixel average).</param>
        /// <param name="width">Frame width in pixels.</param>
        /// <param name="height">Frame height in pixels.</param>
        /// <returns>A new <see cref="CameraFrame"/> instance.</returns>
        public static CameraFrame FromYCbCr422(byte[] data, int width, int height)
        {
            return new CameraFrame(data, width, height, width * 2, OutputFormat.YCbCr422);
        }

        /// <summary>
        /// Gets the color of a pixel at the specified coordinates.
        /// For RGB565 format, converts directly to <see cref="Color"/>.
        /// For YCbCr 4:2:2 format, converts from YCbCr to RGB color space.
        /// </summary>
        /// <param name="x">X coordinate (column).</param>
        /// <param name="y">Y coordinate (row).</param>
        /// <returns>The pixel color.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are outside the frame dimensions.</exception>
        public Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (_format == OutputFormat.Rgb565)
            {
                return GetPixelRgb565(x, y);
            }
            else if (_format == OutputFormat.YCbCr422)
            {
                return GetPixelYCbCr422(x, y);
            }

            // Grayscale: return single-channel Y value
            int offset = (y * Stride) + x;
            byte value = _data[offset];
            return Color.FromArgb(255, value, value, value);
        }

        /// <summary>
        /// Sets the color of a pixel at the specified coordinates.
        /// </summary>
        /// <param name="x">X coordinate (column).</param>
        /// <param name="y">Y coordinate (row).</param>
        /// <param name="color">The color to set.</param>
        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return;
            }

            if (_format == OutputFormat.Rgb565)
            {
                SetPixelRgb565(x, y, color);
            }
            else if (_format == OutputFormat.YCbCr422)
            {
                SetPixelYCbCr422(x, y, color);
            }
        }

        private static Color YCbCrToRgb(byte y, byte cb, byte cr)
        {
            // ITU-R BT.601 conversion
            int c = y - 16;
            int d = cb - 128;
            int e = cr - 128;

            int r = ((298 * c) + (409 * e) + 128) >> 8;
            int g = ((298 * c) - (100 * d) - (208 * e) + 128) >> 8;
            int b = ((298 * c) + (516 * d) + 128) >> 8;

            return Color.FromArgb(255, Clamp(r, 0, 255), Clamp(g, 0, 255), Clamp(b, 0, 255));
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private Color GetPixelRgb565(int x, int y)
        {
            int offset = (y * Stride) + (x * 2);
            byte hi = _data[offset];
            byte lo = _data[offset + 1];

            // RGB565: RRRRRGGG GGGBBBBB
            int r = (hi >> 3) & 0x1F;
            int g = ((hi & 0x07) << 3) | ((lo >> 5) & 0x07);
            int b = lo & 0x1F;

            // Scale from 5/6 bits to 8 bits
            r = (r << 3) | (r >> 2);
            g = (g << 2) | (g >> 4);
            b = (b << 3) | (b >> 2);

            return Color.FromArgb(255, r, g, b);
        }

        private void SetPixelRgb565(int x, int y, Color color)
        {
            int offset = (y * Stride) + (x * 2);

            // Pack into RGB565
            byte r5 = (byte)(color.R >> 3);
            byte g6 = (byte)(color.G >> 2);
            byte b5 = (byte)(color.B >> 3);

            _data[offset] = (byte)((r5 << 3) | (g6 >> 3));
            _data[offset + 1] = (byte)((g6 << 5) | b5);
        }

        private Color GetPixelYCbCr422(int x, int y)
        {
            // YCbCr 4:2:2 byte order: Y0 Cb Y1 Cr (for each pair of pixels)
            int pairOffset = (y * Stride) + ((x & ~1) * 2);

            byte yVal = _data[pairOffset + ((x & 1) == 0 ? 0 : 2)];
            byte cb = _data[pairOffset + 1];
            byte cr = _data[pairOffset + 3];

            return YCbCrToRgb(yVal, cb, cr);
        }

        private void SetPixelYCbCr422(int x, int y, Color color)
        {
            // Set Y component for the specific pixel, Cb/Cr shared per pixel pair
            int pairOffset = (y * Stride) + ((x & ~1) * 2);
            int yOffset = pairOffset + ((x & 1) == 0 ? 0 : 2);

            // Convert RGB to Y
            int yVal = (((66 * color.R) + (129 * color.G) + (25 * color.B) + 128) >> 8) + 16;
            _data[yOffset] = (byte)Clamp(yVal, 0, 255);

            // Set Cb/Cr from this pixel (approximate, shared with neighbor)
            int cb = (((-38 * color.R) - (74 * color.G) + (112 * color.B) + 128) >> 8) + 128;
            int cr = (((112 * color.R) - (94 * color.G) - (18 * color.B) + 128) >> 8) + 128;
            _data[pairOffset + 1] = (byte)Clamp(cb, 0, 255);
            _data[pairOffset + 3] = (byte)Clamp(cr, 0, 255);
        }
    }
}
