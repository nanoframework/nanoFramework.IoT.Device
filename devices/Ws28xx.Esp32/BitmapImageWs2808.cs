// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// BitmapImage Ws2808.
    /// </summary>
    public class BitmapImageWs2808 : BitmapImage
    {
        private const int BytesPerPixel = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitmapImageWs2808"/> class.
        /// </summary>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        public BitmapImageWs2808(int width, int height)
            : base(new byte[width * height * BytesPerPixel], width, height, width * BytesPerPixel)
        {
        }

        /// <inheritdoc />
        public override void SetPixel(int x, int y, Color c)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Data[offset++] = c.R;
            Data[offset++] = c.G;
            Data[offset++] = c.B;
        }

        /// <inheritdoc />
        public override void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Data[offset++] = r;
            Data[offset++] = g;
            Data[offset++] = b;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        /// <inheritdoc />
        public override void Clear(int x, int y)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Array.Clear(Data, offset, 3);
        }
    }
}