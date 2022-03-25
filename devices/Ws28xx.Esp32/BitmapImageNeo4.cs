﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace Iot.Device.Ws28xx.Esp32
{
    internal class BitmapImageNeo4 : BitmapImage
    {
        private const int BytesPerComponentInternal = 3;
        private const int BytesPerPixel = BytesPerComponentInternal * 4;

        // The Neo Pixels require a 50us delay (all zeros) after. Since Spi freq is not exactly
        // as requested 100us is used here with good practical results. 100us @ 2.4Mbps and 8bit
        // data means we have to add 30 bytes of zero padding.
        private const int ResetDelayInBytes = 30;

        // This field defines the count within the lookup table. The length correlates to the possible values of a single byte.
        private const int LookupCount = 256;

        private static readonly byte[] _lookup = new byte[LookupCount * BytesPerComponentInternal];

        static BitmapImageNeo4()
        {
            ClearInternal();
        }

        public BitmapImageNeo4(int width, int height)
                            : base(new byte[width * height * BytesPerPixel], width, height, width * BytesPerPixel)
        {
            BytesPerComponent = BytesPerComponentInternal;
        }

        public override void Clear() => ClearInternal();

        private static void ClearInternal()
        {
            for (int i = 0; i < LookupCount; i++)
            {
                int data = 0;
                for (int j = 7; j >= 0; j--)
                {
                    data = (data << 3) | 0b100 | ((i >> j) << 1) & 2;
                }

                _lookup[i * BytesPerComponentInternal + 0] = unchecked((byte)(data >> 16));
                _lookup[i * BytesPerComponentInternal + 1] = unchecked((byte)(data >> 8));
                _lookup[i * BytesPerComponentInternal + 2] = unchecked((byte)(data >> 0));
            }
        }

        public override void Clear(int x, int y) => SetPixel(x, y, Color.Black);

        public override void SetPixel(int x, int y, Color c)
        {
            // Alpha is used as white.
            var offset = y * Stride + x * BytesPerPixel;
            Data[offset++] = _lookup[c.G * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.G * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.G * BytesPerComponent + 2];
            Data[offset++] = _lookup[c.R * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.R * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.R * BytesPerComponent + 2];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.B * BytesPerComponent + 2];
            Data[offset++] = _lookup[c.A * BytesPerComponent + 0];
            Data[offset++] = _lookup[c.A * BytesPerComponent + 1];
            Data[offset++] = _lookup[c.A * BytesPerComponent + 2];
        }

        public override void SetPixel(int x, int y, byte r, byte g, byte b) => SetPixel(x, y, Color.FromArgb(r, g, b));
    }
}
