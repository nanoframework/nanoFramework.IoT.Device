// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device.Ws28xx
{
    /// <summary>
    /// Represents bitmap image.
    /// </summary>
    public abstract class BitmapImage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapImage" /> class.
        /// </summary>
        /// <param name="data">Data representing the image (derived class defines a specific format).</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="stride">Number of bytes per row.</param>
        protected BitmapImage(byte[] data, int width, int height, int stride)
        {
            _data = data;
            Width = width;
            Height = height;
            Stride = stride;
        }

        private byte[] _data;

        /// <summary>
        /// Data related to the image (derived class defines a specific format).
        /// </summary>
        public byte[] Data => _data;

        /// <summary>
        /// Gets width of the image.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets height of the image.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Gets number of bytes per row.
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// Sets pixel at specific position.
        /// </summary>
        /// <param name="x">X coordinate of the pixel.</param>
        /// <param name="y">Y coordinate of the pixel.</param>
        /// <param name="color">Color to set the pixel to.</param>
        public abstract void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Sets pixel at specific position.
        /// </summary>
        /// <param name="x">X coordinate of the pixel.</param>
        /// <param name="y">Y coordinate of the pixel.</param>
        /// <param name="r">Red color.</param>
        /// <param name="g">Green color.</param>
        /// <param name="b">Blue color.</param>
        public abstract void SetPixel(int x, int y, byte r, byte g, byte b);

        /// <summary>
        /// Clears the image to specific color.
        /// </summary>
        /// <param name="color">Color to clear the image. Defaults to black.</param>
        public virtual void Clear(Color color)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetPixel(x, y, color);
                }
            }
        }

        /// <summary>
        /// Clears whole image.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Clears selected pixel.
        /// </summary>
        /// <param name="x">X coordinate of the pixel.</param>
        /// <param name="y">Y coordinate of the pixel.</param>
        public abstract void Clear(int x, int y);
    }
}