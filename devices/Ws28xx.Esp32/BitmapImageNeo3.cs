// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Drawing;

namespace Iot.Device.Ws28xx.Esp32
{
    /// <summary>
    /// Special 24bit RGB format for Neo pixel LEDs where each bit is converted to 3 bits.
    /// A one is converted to 110, a zero is converted to 100.
    /// </summary>
    internal class BitmapImageNeo3 : BitmapImage
    {
        protected const int BytesPerComponent = 3;
        protected const int BytesPerPixel = BytesPerComponent * 3;

        static BitmapImageNeo3()
        {
            for (int i = 0; i < 256; i++)
            {
                int data = 0;
                for (int j = 7; j >= 0; j--)
                {
                    data = (data << 3) | 0b100 | ((i >> j) << 1) & 2;
                }

                Lookup[(i * BytesPerComponent) + 0] = unchecked((byte)(data >> 16));
                Lookup[(i * BytesPerComponent) + 1] = unchecked((byte)(data >> 8));
                Lookup[(i * BytesPerComponent) + 2] = unchecked((byte)(data >> 0));
            }
        }

        public BitmapImageNeo3(int width, int height)
            : base(new byte[width * height * BytesPerPixel], width, height, width * BytesPerPixel)
        {
        }

        public override void SetPixel(int x, int y, Color c)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.G * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.R * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(c.B * BytesPerComponent) + 2];
        }

        public override void Clear(int x, int y)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Array.Clear(Data, offset, 9);
        }

        public override void Clear()
        {
            Array.Clear(Data, 0, Data.Length);
        }

        public override void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            var offset = (y * Stride) + (x * BytesPerPixel);
            Data[offset++] = Lookup[(g * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(g * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(g * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(r * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(r * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(r * BytesPerComponent) + 2];
            Data[offset++] = Lookup[(b * BytesPerComponent) + 0];
            Data[offset++] = Lookup[(b * BytesPerComponent) + 1];
            Data[offset++] = Lookup[(b * BytesPerComponent) + 2];
        }

        protected static readonly byte[] Lookup = new byte[256 * BytesPerComponent];
    }
}