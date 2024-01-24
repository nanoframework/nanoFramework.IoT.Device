// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device.Ws28xx.Esp32
{
    public class BitmapImageWs2808 : BitmapImage
    {
        private const int BytesPerPixel = 3;

        public BitmapImageWs2808(int width, int height)
            : base(new byte[width * height * BytesPerPixel], width, height, width * BytesPerPixel)
        {
        }

        public override void SetPixel(int x, int y, Color c)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Data[offset++] = c.R;
            Data[offset++] = c.G;
            Data[offset++] = c.B;
        }

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

        public override void Clear(int x, int y)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Array.Clear(Data, offset, 3);
        }
    }
}